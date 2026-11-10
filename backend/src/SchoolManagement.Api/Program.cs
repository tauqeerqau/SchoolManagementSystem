using SchoolManagement.Application.Students;
using SchoolManagement.Application.Authentication;
using SchoolManagement.Infrastructure;
using SchoolManagement.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevelopment = "AngularDevelopment";

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<RegisterStudent>();
builder.Services.AddControllers();
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

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(AngularDevelopment);

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
