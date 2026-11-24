using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolManagement.Application.Authentication;
using SchoolManagement.Infrastructure.Identity;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Identity;

public sealed class IdentityUserAuthenticatorTests
{
    [Fact]
    public async Task AuthenticateAsync_WithValidCredentialsReturnsUserDetailsAndRoles()
    {
        var store = new InMemoryIdentityUserStore();
        var userManager = CreateUserManager(store);
        var registration = new StudentAccountRegistration(userManager);
        await registration.CreateStudentAccountAsync("Ava", "Patel", "ava.patel@example.com", "V3ryStr0ng!Pass", CancellationToken.None);
        var authenticateUser = new AuthenticateUser(new IdentityUserAuthenticator(userManager));

        var result = await authenticateUser.ExecuteAsync(
            new LoginRequest("ava.patel@example.com", "V3ryStr0ng!Pass"),
            CancellationToken.None);

        var user = Assert.Single(store.Users);
        Assert.Equal(LoginStatus.Succeeded, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(user.Id, result.Response.UserId);
        Assert.Equal("ava.patel@example.com", result.Response.Email);
        Assert.Equal("Ava", result.Response.FirstName);
        Assert.Equal("Patel", result.Response.LastName);
        Assert.Equal(["STUDENT"], result.Response.Roles);
        Assert.DoesNotContain("Password", typeof(LoginResponse).GetProperties().Select(p => p.Name));
        Assert.DoesNotContain("PasswordHash", typeof(LoginResponse).GetProperties().Select(p => p.Name));
    }

    [Theory]
    [InlineData("missing@example.com", "V3ryStr0ng!Pass")]
    [InlineData("ava.patel@example.com", "incorrect-password")]
    public async Task AuthenticateAsync_WithUnknownEmailOrWrongPasswordReturnsSameFailure(
        string email,
        string password)
    {
        var store = new InMemoryIdentityUserStore();
        var userManager = CreateUserManager(store);
        var registration = new StudentAccountRegistration(userManager);
        await registration.CreateStudentAccountAsync("Ava", "Patel", "ava.patel@example.com", "V3ryStr0ng!Pass", CancellationToken.None);
        var authenticateUser = new AuthenticateUser(new IdentityUserAuthenticator(userManager));

        var result = await authenticateUser.ExecuteAsync(
            new LoginRequest(email, password),
            CancellationToken.None);

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
        Assert.Null(result.Response);
    }

    private static UserManager<ApplicationUser> CreateUserManager(
        InMemoryIdentityUserStore store)
    {
        var options = new IdentityOptions();
        options.User.RequireUniqueEmail = true;

        return new UserManager<ApplicationUser>(
            store,
            Options.Create(options),
            new PasswordHasher<ApplicationUser>(),
            [new UserValidator<ApplicationUser>()],
            [new PasswordValidator<ApplicationUser>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }
}
