using Microsoft.AspNetCore.Identity;
using SchoolManagement.Application.Authentication;

namespace SchoolManagement.Infrastructure.Identity;

public sealed class IdentityUserAuthenticator(
    UserManager<ApplicationUser> userManager) : IUserAuthenticator
{
    public async Task<LoginResponse?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new LoginResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles.ToArray());
    }
}
