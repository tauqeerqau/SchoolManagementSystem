using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SchoolManagement.Api.Authentication;
using SchoolManagement.Api.Controllers;
using SchoolManagement.Application.Authentication;

namespace SchoolManagement.UnitTests.Api;

public sealed class AuthenticationControllerLoginTests
{
    [Fact]
    public async Task Login_WithValidCredentialsReturnsOkWithTokenAndUserInformation()
    {
        var authenticatedUser = new LoginResponse(
            "user-123",
            "ava.patel@example.com",
            "Ava",
            "Patel",
            ["Student", "Teacher"]);
        var authenticator = new FakeUserAuthenticator(authenticatedUser);
        var tokenGenerator = new FakeAccessTokenGenerator(
            new GeneratedAccessToken("signed.jwt.token", DateTimeOffset.UtcNow.AddMinutes(20)));
        var controller = CreateController(authenticator, tokenGenerator);
        using var cancellation = new CancellationTokenSource();

        var actionResult = await controller.Login(
            new LoginRequest("ava.patel@example.com", "secret-password"),
            cancellation.Token);

        var ok = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<LoginApiResponse>(ok.Value);
        Assert.Equal("signed.jwt.token", response.AccessToken);
        Assert.Equal(tokenGenerator.TokenResult.ExpiresAtUtc, response.ExpiresAt);
        Assert.Equal("user-123", response.UserId);
        Assert.Equal("ava.patel@example.com", response.Email);
        Assert.Equal("Ava", response.FirstName);
        Assert.Equal("Patel", response.LastName);
        Assert.Equal(new[] { "Student", "Teacher" }, response.Roles);
        Assert.Equal(1, tokenGenerator.CallCount);
        Assert.Equal("ava.patel@example.com", authenticator.Email);
        Assert.Equal("secret-password", authenticator.Password);
        Assert.Equal(cancellation.Token, authenticator.CancellationToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentialsReturnsSameGenericUnauthorizedResponse()
    {
        var tokenGenerator = new FakeAccessTokenGenerator(
            new GeneratedAccessToken("should-not-be-issued", DateTimeOffset.UtcNow));
        var existingEmailController = CreateController(
            new FakeUserAuthenticator(null),
            tokenGenerator);
        var unknownEmailController = CreateController(
            new FakeUserAuthenticator(null),
            tokenGenerator);

        var existingEmailResult = await existingEmailController.Login(
            new LoginRequest("known@example.com", "wrong-password"),
            CancellationToken.None);
        var unknownEmailResult = await unknownEmailController.Login(
            new LoginRequest("unknown@example.com", "wrong-password"),
            CancellationToken.None);

        var existingEmailUnauthorized = Assert.IsType<UnauthorizedObjectResult>(existingEmailResult);
        var unknownEmailUnauthorized = Assert.IsType<UnauthorizedObjectResult>(unknownEmailResult);
        var existingEmailProblem = Assert.IsType<ProblemDetails>(existingEmailUnauthorized.Value);
        var unknownEmailProblem = Assert.IsType<ProblemDetails>(unknownEmailUnauthorized.Value);

        Assert.Equal(StatusCodes.Status401Unauthorized, existingEmailUnauthorized.StatusCode);
        Assert.Equal(StatusCodes.Status401Unauthorized, unknownEmailUnauthorized.StatusCode);
        Assert.Equal(existingEmailProblem.Title, unknownEmailProblem.Title);
        Assert.Equal(existingEmailProblem.Detail, unknownEmailProblem.Detail);
        Assert.Equal("Invalid credentials.", existingEmailProblem.Title);
        Assert.Equal("Email or password is incorrect.", existingEmailProblem.Detail);
        Assert.Equal(0, tokenGenerator.CallCount);
    }

    private static AuthenticationController CreateController(
        FakeUserAuthenticator authenticator,
        FakeAccessTokenGenerator tokenGenerator) =>
        new(
            new RegisterStudent(new UnusedStudentAccountRegistration()),
            new AuthenticateUser(authenticator),
            tokenGenerator);

    private sealed class UnusedStudentAccountRegistration : IStudentAccountRegistration
    {
        public Task<StudentAccountRegistrationResult> CreateStudentAccountAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            CancellationToken cancellationToken) =>
            Task.FromResult(StudentAccountRegistrationResult.Failed());
    }

    private sealed class FakeUserAuthenticator(LoginResponse? response) : IUserAuthenticator
    {
        public string? Email { get; private set; }

        public string? Password { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<LoginResponse?> AuthenticateAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            Email = email;
            Password = password;
            CancellationToken = cancellationToken;
            return Task.FromResult(response);
        }
    }

    private sealed class FakeAccessTokenGenerator(GeneratedAccessToken tokenResult)
        : IAccessTokenGenerator
    {
        public int CallCount { get; private set; }

        public GeneratedAccessToken TokenResult => tokenResult;

        public GeneratedAccessToken Generate(
            string userId,
            string email,
            string firstName,
            string lastName,
            IReadOnlyCollection<string> roles)
        {
            CallCount++;
            Assert.Equal("user-123", userId);
            Assert.Equal("ava.patel@example.com", email);
            Assert.Equal("Ava", firstName);
            Assert.Equal("Patel", lastName);
            Assert.Equal(new[] { "Student", "Teacher" }, roles);
            return tokenResult;
        }
    }
}
