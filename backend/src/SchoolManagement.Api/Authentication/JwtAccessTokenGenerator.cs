using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SchoolManagement.Api.Authentication;

public sealed class JwtAccessTokenGenerator(IOptions<JwtSettings> options)
    : IAccessTokenGenerator
{
    private readonly JwtSettings _settings = options.Value;

    public GeneratedAccessToken Generate(
        string userId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> roles)
    {
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_settings.AccessTokenExpirationMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.GivenName, firstName),
            new(ClaimTypes.Surname, lastName),
            new(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        return new GeneratedAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            new DateTimeOffset(DateTime.SpecifyKind(token.ValidTo, DateTimeKind.Utc)));
    }
}
