using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.UnitTests.Fakes;

internal sealed class InMemoryIdentityUserStore :
    IUserStore<SchoolManagement.Infrastructure.Identity.ApplicationUser>,
    IUserPasswordStore<SchoolManagement.Infrastructure.Identity.ApplicationUser>,
    IUserEmailStore<SchoolManagement.Infrastructure.Identity.ApplicationUser>,
    IUserRoleStore<SchoolManagement.Infrastructure.Identity.ApplicationUser>
{
    private readonly List<SchoolManagement.Infrastructure.Identity.ApplicationUser> _users = [];
    private readonly Dictionary<string, HashSet<string>> _roles = new(StringComparer.OrdinalIgnoreCase);

    public bool FailRoleAssignment { get; set; }

    public IReadOnlyList<SchoolManagement.Infrastructure.Identity.ApplicationUser> Users => _users;

    public Task<IdentityResult> CreateAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken)
    {
        _users.Add(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> DeleteAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken)
    {
        _roles.Remove(user.Id);
        _users.Remove(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<string> GetUserIdAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.UserName);

    public Task SetUserNameAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string? userName,
        CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

    public Task<SchoolManagement.Infrastructure.Identity.ApplicationUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken) => Task.FromResult(
            _users.FirstOrDefault(user => user.Id == userId));

    public Task<SchoolManagement.Infrastructure.Identity.ApplicationUser?> FindByNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken) => Task.FromResult(
            _users.FirstOrDefault(user => user.NormalizedUserName == normalizedUserName));

    public Task SetNormalizedUserNameAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string? normalizedName,
        CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string? passwordHash,
        CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash is not null);

    public Task SetEmailAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string? email,
        CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        bool confirmed,
        CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<SchoolManagement.Infrastructure.Identity.ApplicationUser?> FindByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken) => Task.FromResult(
            _users.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail));

    public Task<string?> GetNormalizedEmailAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string? normalizedEmail,
        CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task AddToRoleAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        if (FailRoleAssignment)
        {
            throw new InvalidOperationException("Simulated role-store failure.");
        }

        if (!_roles.TryGetValue(user.Id, out var roles))
        {
            roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _roles[user.Id] = roles;
        }

        roles.Add(normalizedRoleName);
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        if (_roles.TryGetValue(user.Id, out var roles))
        {
            roles.Remove(normalizedRoleName);
        }

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        CancellationToken cancellationToken)
    {
        IList<string> roles = _roles.TryGetValue(user.Id, out var userRoles)
            ? userRoles.ToList()
            : new List<string>();
        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(
        SchoolManagement.Infrastructure.Identity.ApplicationUser user,
        string normalizedRoleName,
        CancellationToken cancellationToken) => Task.FromResult(
            _roles.TryGetValue(user.Id, out var roles) && roles.Contains(normalizedRoleName));

    public Task<IList<SchoolManagement.Infrastructure.Identity.ApplicationUser>> GetUsersInRoleAsync(
        string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        IList<SchoolManagement.Infrastructure.Identity.ApplicationUser> users = _users
            .Where(user => _roles.TryGetValue(user.Id, out var roles)
                && roles.Contains(normalizedRoleName))
            .ToList();
        return Task.FromResult(users);
    }

    public void Dispose()
    {
    }
}
