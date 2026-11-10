using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Infrastructure.Identity;

public sealed class IdentityRoleProvisioner(RoleManager<IdentityRole> roleManager)
    : IIdentityRoleProvisioner
{
    public async Task ProvisionAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in IdentityRoleNames.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded && !await roleManager.RoleExistsAsync(roleName))
            {
                throw new InvalidOperationException("Identity role provisioning failed.");
            }
        }
    }
}
