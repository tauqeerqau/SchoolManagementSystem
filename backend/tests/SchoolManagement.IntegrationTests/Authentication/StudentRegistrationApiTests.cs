using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolManagement.Application.Authentication;
using SchoolManagement.IntegrationTests.Infrastructure;

namespace SchoolManagement.IntegrationTests.Authentication;

public sealed class StudentRegistrationApiTests
{
    [Fact]
    public async Task PostRegister_Returns201AndSafeResponse()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.Created("student-user-42"));
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<StudentRegistrationResponse>();
        Assert.Equal(
            new StudentRegistrationResponse(
                "student-user-42",
                "Ava",
                "Patel",
                "ava.patel@example.com"),
            body);

        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Role", json, StringComparison.OrdinalIgnoreCase);
        Assert.True(registration.Called);
        Assert.Equal("ava.patel@example.com", registration.Email);
    }

    [Fact]
    public async Task PostRegister_PasswordMismatchReturns400()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.Created("unused"));
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();
        var request = ValidRequest();
        request["ConfirmPassword"] = "Different!Pass1";

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(registration.Called);
    }

    [Fact]
    public async Task PostRegister_InvalidEmailReturns400()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.Created("unused"));
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();
        var request = ValidRequest();
        request["Email"] = "not-an-email";

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(registration.Called);
    }

    [Fact]
    public async Task PostRegister_MissingRequiredFieldsReturns400()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.Created("unused"));
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new Dictionary<string, string>());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(registration.Called);
    }

    [Fact]
    public async Task PostRegister_WeakPasswordReturnsSafe400()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.ValidationFailed(
                new Dictionary<string, string[]>
                {
                    ["Password"] = ["The password does not meet the security requirements."]
                }));
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", ValidRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("security requirements", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordTooShort", json, StringComparison.Ordinal);
        Assert.DoesNotContain("IdentityError", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostRegister_DuplicateEmailReturnsGenericConflict()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.DuplicateEmail());
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", ValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Registration could not be completed", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ava.patel@example.com", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("already registered", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostRegister_CannotSelectAdminRoleFromRequest()
    {
        var registration = new FakeStudentAccountRegistration(
            StudentAccountRegistrationResult.Created("student-user-42"));
        using var factory = CreateFactory(registration);
        using var client = factory.CreateClient();
        var request = ValidRequest();
        request["Role"] = "Admin";

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Null(typeof(StudentRegistrationRequest).GetProperty("Role"));
        Assert.True(registration.Called);
    }

    private static WebApplicationFactory<global::Program> CreateFactory(
        FakeStudentAccountRegistration registration) =>
        new SchoolManagementApiFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IStudentAccountRegistration>();
                services.AddSingleton<IStudentAccountRegistration>(registration);
            }));

    private static Dictionary<string, string> ValidRequest() => new()
    {
        ["FirstName"] = "Ava",
        ["LastName"] = "Patel",
        ["Email"] = "ava.patel@example.com",
        ["Password"] = "V3ryStr0ng!Pass",
        ["ConfirmPassword"] = "V3ryStr0ng!Pass"
    };

    private sealed class FakeStudentAccountRegistration(
        StudentAccountRegistrationResult result) : IStudentAccountRegistration
    {
        public bool Called { get; private set; }

        public string? Email { get; private set; }

        public string RequestedFields { get; private set; } = string.Empty;

        public Task<StudentAccountRegistrationResult> CreateStudentAccountAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            Called = true;
            Email = email;
            RequestedFields = $"{firstName} {lastName} {email}";
            return Task.FromResult(result);
        }
    }
}
