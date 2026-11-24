using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Api.Authentication;

namespace SchoolManagement.UnitTests.Api;

public sealed class JwtAccessTokenGeneratorTests
{
    private const string SigningKey = "local-test-signing-key-with-at-least-32-bytes";
    private const string Issuer = "SchoolManagement.Api.Tests";
    private const string Audience = "SchoolManagement.AngularClient.Tests";

    [Fact]
    public void Generate_CreatesSignedTokenWithUserRolesAndConfiguredExpiry()
    {
        var generator = CreateGenerator(SigningKey);
        var startedAt = DateTimeOffset.UtcNow;

        var generated = generator.Generate(
            "user-123",
            "ava.patel@example.com",
            "Ava",
            "Patel",
            ["Student", "Teacher"]);

        var principal = ValidateToken(generated.Token, SigningKey);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(generated.Token);

        Assert.Equal("user-123", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("ava.patel@example.com", principal.FindFirstValue(ClaimTypes.Email));
        Assert.Equal("Ava", principal.FindFirstValue(ClaimTypes.GivenName));
        Assert.Equal("Patel", principal.FindFirstValue(ClaimTypes.Surname));
        Assert.Equal(
            new[] { "Student", "Teacher" },
            principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).OrderBy(role => role));
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Contains(Audience, jwt.Audiences);
        Assert.True(jwt.IssuedAt >= startedAt.UtcDateTime.AddSeconds(-1));
        Assert.True(jwt.IssuedAt <= DateTime.UtcNow.AddSeconds(1));
        Assert.Equal(jwt.ValidTo, generated.ExpiresAtUtc.UtcDateTime);
        Assert.InRange(
            generated.ExpiresAtUtc,
            startedAt.AddMinutes(20).AddSeconds(-2),
            DateTimeOffset.UtcNow.AddMinutes(20).AddSeconds(2));
    }

    [Fact]
    public void Generate_TokenSignatureRequiresConfiguredSigningKey()
    {
        var generator = CreateGenerator(SigningKey);
        var generated = generator.Generate(
            "user-123",
            "ava.patel@example.com",
            "Ava",
            "Patel",
            ["Student"]);

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(
            () => ValidateToken(generated.Token, "a-different-signing-key-with-32-bytes-minimum"));
    }

    private static JwtAccessTokenGenerator CreateGenerator(string signingKey) =>
        new(Options.Create(new JwtSettings
        {
            Issuer = Issuer,
            Audience = Audience,
            AccessTokenExpirationMinutes = 20,
            SigningKey = signingKey
        }));

    private static ClaimsPrincipal ValidateToken(string token, string signingKey)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
            },
            out _);
    }
}
