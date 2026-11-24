using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SchoolManagement.Api.Authentication;
using SchoolManagement.Application.Authentication;
using SchoolManagement.Application.Students;

namespace SchoolManagement.IntegrationTests.Authentication;

public sealed class StudentApiAuthorizationTests
{
    private const string LoginEmail = "student@example.com";
    private const string LoginPassword = "V3ryStr0ng!Pass";

    [Fact]
    public async Task StudentEndpoints_WithoutTokenReturnUnauthorized()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClient();

        var responses = await SendAllStudentRequestsAsync(client);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Unauthorized, response));
    }

    [Fact]
    public async Task GetStudents_WithInvalidTokenReturnsUnauthorized()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "invalid.token.value");

        using var response = await client.GetAsync("/api/students");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StudentEndpoints_WithStudentTokenReturnForbidden()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(LoginEmail, LoginPassword));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginApiResponse>();
        Assert.NotNull(login);
        Assert.Contains("Student", login.Roles);
        Assert.DoesNotContain("Admin", login.Roles);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var responses = await SendAllStudentRequestsAsync(client);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Forbidden, response));
    }

    [Fact]
    public async Task StudentEndpoints_TeacherCanReadButCannotWrite()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClientWithRoles("Teacher");

        var responses = await SendAllStudentRequestsAsync(client);

        Assert.Equal(
            [HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden,
                HttpStatusCode.Forbidden, HttpStatusCode.Forbidden],
            responses);
    }

    [Fact]
    public async Task StudentEndpoints_AdminCanPerformAllOperations()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClientWithRoles("Admin");

        var responses = await SendAllStudentRequestsAsync(client);

        Assert.Equal(
            [HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Created,
                HttpStatusCode.OK, HttpStatusCode.NoContent],
            responses);
    }

    [Fact]
    public async Task Registration_RemainsAccessibleWithoutToken()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new StudentRegistrationRequest
            {
                FirstName = "Taylor",
                LastName = "Student",
                Email = "taylor@example.com",
                Password = LoginPassword,
                ConfirmPassword = LoginPassword
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Login_RemainsAccessibleWithoutToken()
    {
        using var factory = new ProtectedApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(LoginEmail, LoginPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<HttpStatusCode[]> SendAllStudentRequestsAsync(HttpClient client)
    {
        var responses = new List<HttpStatusCode>();
        using (var getAll = await client.GetAsync("/api/students"))
        {
            responses.Add(getAll.StatusCode);
        }

        using (var getById = await client.GetAsync("/api/students/1"))
        {
            responses.Add(getById.StatusCode);
        }

        using (var create = await client.PostAsJsonAsync("/api/students", CreateStudentRequest()))
        {
            responses.Add(create.StatusCode);
        }

        using (var update = await client.PutAsJsonAsync("/api/students/1", CreateStudentRequest()))
        {
            responses.Add(update.StatusCode);
        }

        using (var delete = await client.DeleteAsync("/api/students/1"))
        {
            responses.Add(delete.StatusCode);
        }

        return responses.ToArray();
    }

    private static StudentCreateRequest CreateStudentRequest() => new()
    {
        FirstName = "Riley",
        LastName = "Student",
        Email = "riley@example.com",
        DateOfBirth = new DateTime(2008, 3, 12),
        EnrollmentDate = new DateTime(2025, 9, 1),
        IsActive = true
    };

    private sealed class ProtectedApiFactory : WebApplicationFactory<global::Program>
    {
        public HttpClient CreateClientWithRoles(params string[] roles)
        {
            var client = CreateClient();
            var generator = Services.GetRequiredService<IAccessTokenGenerator>();
            var token = generator.Generate(
                "test-user",
                "test@example.com",
                "Test",
                "User",
                roles);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SigningKey"] = "student-api-test-signing-key-at-least-32-bytes"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IStudentService>();
                services.AddSingleton<IStudentService, FakeStudentService>();
                services.RemoveAll<IStudentAccountRegistration>();
                services.AddSingleton<IStudentAccountRegistration, FakeStudentAccountRegistration>();
                services.RemoveAll<IUserAuthenticator>();
                services.AddSingleton<IUserAuthenticator, FakeUserAuthenticator>();
            });
        }
    }

    private sealed class FakeStudentService : IStudentService
    {
        public Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<StudentDto>>([]);

        public Task<StudentDto?> GetByIdAsync(int studentId, CancellationToken cancellationToken) =>
            Task.FromResult<StudentDto?>(CreateStudentDto(studentId));

        public Task<StudentDto> CreateAsync(
            StudentCreateRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(CreateStudentDto(2));

        public Task<StudentDto?> UpdateAsync(
            int studentId,
            StudentUpdateRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult<StudentDto?>(CreateStudentDto(studentId));

        public Task<bool> DeleteAsync(int studentId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        private static StudentDto CreateStudentDto(int id) => new(
            id,
            "Riley",
            "Student",
            "riley@example.com",
            new DateTime(2008, 3, 12),
            new DateTime(2025, 9, 1),
            true);
    }

    private sealed class FakeStudentAccountRegistration : IStudentAccountRegistration
    {
        public Task<StudentAccountRegistrationResult> CreateStudentAccountAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            CancellationToken cancellationToken) =>
            Task.FromResult(StudentAccountRegistrationResult.Created("registered-user"));
    }

    private sealed class FakeUserAuthenticator : IUserAuthenticator
    {
        public Task<LoginResponse?> AuthenticateAsync(
            string email,
            string password,
            CancellationToken cancellationToken) =>
            Task.FromResult<LoginResponse?>(
                email == LoginEmail && password == LoginPassword
                    ? new LoginResponse(
                        "student-user",
                        LoginEmail,
                        "Taylor",
                        "Student",
                        ["Student"])
                    : null);
    }
}
