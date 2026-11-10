using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolManagement.Application.Authentication;
using SchoolManagement.Infrastructure.Identity;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Identity;

public sealed class StudentAccountRegistrationTests
{
    [Fact]
    public async Task CreateStudentAccountAsync_CreatesHashedPasswordAndAssignsOnlyStudentRole()
    {
        var store = new InMemoryIdentityUserStore();
        var userManager = CreateUserManager(store);
        var registration = new StudentAccountRegistration(userManager);

        var result = await registration.CreateStudentAccountAsync(
            "Ava",
            "Patel",
            "ava.patel@example.com",
            "V3ryStr0ng!Pass",
            CancellationToken.None);

        var user = Assert.Single(store.Users);
        Assert.Equal(StudentAccountRegistrationStatus.Created, result.Status);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Ava", user.FirstName);
        Assert.Equal("Patel", user.LastName);
        Assert.Equal("ava.patel@example.com", user.UserName);
        Assert.Equal("ava.patel@example.com", user.Email);
        Assert.NotEqual("V3ryStr0ng!Pass", user.PasswordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<ApplicationUser>().VerifyHashedPassword(
                user,
                user.PasswordHash!,
                "V3ryStr0ng!Pass"));
        Assert.True(await userManager.IsInRoleAsync(user, IdentityRoleNames.Student));
        Assert.False(await userManager.IsInRoleAsync(user, IdentityRoleNames.Admin));
        Assert.False(await userManager.IsInRoleAsync(user, IdentityRoleNames.Teacher));
        Assert.Null(typeof(StudentRegistrationRequest).GetProperty("Role"));
    }

    [Fact]
    public async Task CreateStudentAccountAsync_WeakPasswordReturnsSafeValidationFailure()
    {
        var store = new InMemoryIdentityUserStore();
        var registration = new StudentAccountRegistration(CreateUserManager(store));

        var result = await registration.CreateStudentAccountAsync(
            "Ava",
            "Patel",
            "ava.patel@example.com",
            "weak",
            CancellationToken.None);

        Assert.Equal(StudentAccountRegistrationStatus.ValidationFailed, result.Status);
        Assert.Contains("Password", result.ValidationErrors.Keys);
        Assert.All(
            result.ValidationErrors.Values.SelectMany(messages => messages),
            message => Assert.Equal(
                "The password does not meet the security requirements.",
                message));
        Assert.Empty(store.Users);
    }

    [Fact]
    public async Task CreateStudentAccountAsync_DuplicateEmailReturnsGenericDuplicateOutcome()
    {
        var store = new InMemoryIdentityUserStore();
        var registration = new StudentAccountRegistration(CreateUserManager(store));

        var firstResult = await registration.CreateStudentAccountAsync(
            "Ava",
            "Patel",
            "ava.patel@example.com",
            "V3ryStr0ng!Pass",
            CancellationToken.None);
        var duplicateResult = await registration.CreateStudentAccountAsync(
            "Another",
            "Person",
            "AVA.PATEL@example.com",
            "V3ryStr0ng!Pass",
            CancellationToken.None);

        Assert.Equal(StudentAccountRegistrationStatus.Created, firstResult.Status);
        Assert.Equal(StudentAccountRegistrationStatus.DuplicateEmail, duplicateResult.Status);
        Assert.Empty(duplicateResult.ValidationErrors);
        Assert.Single(store.Users);
    }

    [Fact]
    public async Task CreateStudentAccountAsync_RoleAssignmentFailureRemovesCreatedUser()
    {
        var store = new InMemoryIdentityUserStore { FailRoleAssignment = true };
        var registration = new StudentAccountRegistration(CreateUserManager(store));

        var result = await registration.CreateStudentAccountAsync(
            "Ava",
            "Patel",
            "ava.patel@example.com",
            "V3ryStr0ng!Pass",
            CancellationToken.None);

        Assert.Equal(StudentAccountRegistrationStatus.Failed, result.Status);
        Assert.Empty(store.Users);
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
