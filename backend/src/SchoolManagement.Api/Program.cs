using SchoolManagement.Application.Students;
using SchoolManagement.Application.Authentication;
using SchoolManagement.Infrastructure;
using SchoolManagement.Infrastructure.Identity;
using SchoolManagement.Api.ExceptionHandling;
using SchoolManagement.Api.Authentication;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevelopment = "AngularDevelopment";

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<RegisterStudent>();
builder.Services.AddScoped<AuthenticateUser>();
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), "JWT issuer is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), "JWT audience is required.")
    .Validate(
        settings => settings.AccessTokenExpirationMinutes is > 0 and <= 1440,
        "JWT access token expiration must be between 1 and 1440 minutes.")
    .Validate(
        settings => Encoding.UTF8.GetByteCount(settings.SigningKey ?? string.Empty) >= 32,
        "JWT signing key must contain at least 32 UTF-8 bytes.")
    .ValidateOnStart();
builder.Services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
builder.Services.AddJwtBearerAuthentication();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevelopment, policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization");
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(AngularDevelopment);
app.UseAuthentication();
app.UseAuthorization();

if (builder.Configuration.GetValue<bool>("Identity:ProvisionRolesOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var roleProvisioner = scope.ServiceProvider
        .GetRequiredService<IIdentityRoleProvisioner>();
    await roleProvisioner.ProvisionAsync();
}

app.MapControllers();

app.Run();

public partial class Program { }
