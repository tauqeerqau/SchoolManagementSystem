using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Api.Authentication;

namespace SchoolManagement.IntegrationTests.Authentication;

public sealed class JwtBearerAuthenticationTests
{
    private const string SigningKey = "phase-five-integration-test-key-at-least-32-bytes";
    private const string Issuer = "SchoolManagement.Api.Tests";
    private const string Audience = "SchoolManagement.AngularClient.Tests";

    [Fact]
    public async Task GetProtected_WhenTokenIsValidAuthenticatesUserAndRecognizesRoles()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                JwtBearerDefaults.AuthenticationScheme,
                CreateGeneratedToken(["Admin", "Student"]));

        using var response = await client.GetAsync("/protected");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        using var body = await System.Text.Json.JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var payload = body.RootElement;
        Assert.Equal("user-456", payload.GetProperty("userId").GetString());
        Assert.Equal("admin@example.com", payload.GetProperty("email").GetString());
        Assert.True(payload.GetProperty("isAdmin").GetBoolean());
        Assert.Equal(
            new[] { "Admin", "Student" },
            payload.GetProperty("roles").EnumerateArray().Select(role => role.GetString()));
    }

    [Fact]
    public async Task GetProtected_WhenSignatureIsInvalidReturnsUnauthorized()
    {
        await AssertUnauthorizedAsync(CreateSignedToken(key: "different-test-signing-key-with-32-bytes"));
    }

    [Fact]
    public async Task GetProtected_WhenTokenIsExpiredReturnsUnauthorized()
    {
        await AssertUnauthorizedAsync(CreateSignedToken(expiresAt: DateTime.UtcNow.AddMinutes(-5)));
    }

    [Fact]
    public async Task GetProtected_WhenIssuerIsIncorrectReturnsUnauthorized()
    {
        await AssertUnauthorizedAsync(CreateSignedToken(issuer: "unexpected-issuer"));
    }

    [Fact]
    public async Task GetProtected_WhenAudienceIsIncorrectReturnsUnauthorized()
    {
        await AssertUnauthorizedAsync(CreateSignedToken(audience: "unexpected-audience"));
    }

    [Fact]
    public async Task GetProtected_WhenTokenIsMissingReturnsUnauthorized()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();

        using var response = await client.GetAsync("/protected");

        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    private static async Task AssertUnauthorizedAsync(string token)
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                JwtBearerDefaults.AuthenticationScheme,
                token);

        using var response = await client.GetAsync("/protected");

        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    private static async Task<WebApplication> CreateTestAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.Configure<JwtSettings>(settings =>
        {
            settings.Issuer = Issuer;
            settings.Audience = Audience;
            settings.AccessTokenExpirationMinutes = 15;
            settings.SigningKey = SigningKey;
        });
        builder.Services.AddJwtBearerAuthentication();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/protected", (ClaimsPrincipal principal) => Results.Ok(new
        {
            userId = principal.FindFirstValue(ClaimTypes.NameIdentifier),
            email = principal.FindFirstValue(ClaimTypes.Email),
            roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value),
            isAdmin = principal.IsInRole("Admin")
        })).RequireAuthorization();
        await app.StartAsync();
        return app;
    }

    private static string CreateGeneratedToken(IReadOnlyCollection<string> roles)
    {
        var generator = new JwtAccessTokenGenerator(Options.Create(new JwtSettings
        {
            Issuer = Issuer,
            Audience = Audience,
            AccessTokenExpirationMinutes = 15,
            SigningKey = SigningKey
        }));

        return generator.Generate(
            "user-456",
            "admin@example.com",
            "Ada",
            "Administrator",
            roles).Token;
    }

    private static string CreateSignedToken(
        string key = SigningKey,
        string issuer = Issuer,
        string audience = Audience,
        DateTime? expiresAt = null)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-456"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: DateTime.UtcNow.AddMinutes(-10),
            expires: expiresAt ?? DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
