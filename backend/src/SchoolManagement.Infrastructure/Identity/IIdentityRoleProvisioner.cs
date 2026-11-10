namespace SchoolManagement.Infrastructure.Identity;

public interface IIdentityRoleProvisioner
{
    Task ProvisionAsync(CancellationToken cancellationToken = default);
}
