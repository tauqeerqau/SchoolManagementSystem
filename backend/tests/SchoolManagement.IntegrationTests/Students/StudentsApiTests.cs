using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using SchoolManagement.Application.Students;
using SchoolManagement.Domain;
using SchoolManagement.IntegrationTests.Infrastructure;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.IntegrationTests.Students;

public sealed class StudentsApiTests
{
    [Fact]
    public async Task GetAll_WhenTestDatabaseIsEmpty_ReturnsOkJsonArray()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/students");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var students = await response.Content.ReadFromJsonAsync<StudentDto[]>();

        Assert.NotNull(students);
        Assert.Empty(students);
    }

    [Fact]
    public async Task GetById_WhenStudentExists_ReturnsOkWithStudentDto()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
        var dateOfBirth = new DateTime(2009, 4, 17);
        var enrollmentDate = new DateTime(2025, 9, 2);
        var student = new Student
        {
            FirstName = "Amara",
            LastName = "Okafor",
            Email = $"amara.okafor.{Guid.NewGuid():N}@example.com",
            DateOfBirth = dateOfBirth,
            EnrollmentDate = enrollmentDate,
            IsActive = true
        };

        dbContext.Students.Add(student);
        TestDatabaseSafety.EnsureSafeConnectionString(
            dbContext.Database.GetDbConnection().ConnectionString);
        await dbContext.SaveChangesAsync();

        try
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync($"/api/students/{student.StudentId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var actualStudent = await response.Content.ReadFromJsonAsync<StudentDto>();

            Assert.NotNull(actualStudent);
            Assert.Equal(
                new StudentDto(
                    student.StudentId,
                    "Amara",
                    "Okafor",
                    student.Email,
                    dateOfBirth,
                    enrollmentDate,
                    true),
                actualStudent);
        }
        finally
        {
            if (student.StudentId != 0)
            {
                dbContext.Students.Remove(student);
                TestDatabaseSafety.EnsureSafeConnectionString(
                    dbContext.Database.GetDbConnection().ConnectionString);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task GetById_WhenStudentDoesNotExist_ReturnsNotFound()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
        var nonexistentStudentId = Random.Shared.Next(1, int.MaxValue);

        while (await dbContext.Students.AnyAsync(
                   student => student.StudentId == nonexistentStudentId))
        {
            nonexistentStudentId = Random.Shared.Next(1, int.MaxValue);
        }

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"/api/students/{nonexistentStudentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedResource()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);

        var email = CreateUniqueEmail("created");
        var request = CreateValidRequest(email);

        try
        {
            using var client = factory.CreateClient();
            await ValidateTestDatabaseConnectionStringAsync(factory);
            using var response = await client.PostAsJsonAsync("/api/students", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdStudent = await response.Content.ReadFromJsonAsync<StudentDto>();
            Assert.NotNull(createdStudent);
            Assert.True(createdStudent.StudentId > 0);
            Assert.Equal(
                new StudentDto(
                    createdStudent.StudentId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.DateOfBirth!.Value,
                    request.EnrollmentDate!.Value,
                    request.IsActive!.Value),
                createdStudent);

            var location = response.Headers.Location;
            Assert.NotNull(location);
            var resourceLocation = location.IsAbsoluteUri
                ? location
                : new Uri(client.BaseAddress!, location);
            Assert.EndsWith(
                $"/{createdStudent.StudentId}",
                resourceLocation.AbsolutePath,
                StringComparison.OrdinalIgnoreCase);

            using var getResponse = await client.GetAsync(location);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var retrievedStudent = await getResponse.Content.ReadFromJsonAsync<StudentDto>();
            Assert.Equal(createdStudent, retrievedStudent);
        }
        finally
        {
            await CleanupStudentByEmailAsync(factory, email);
        }
    }

    [Fact]
    public async Task Create_WhenRequiredFieldsAreMissing_ReturnsBadRequestWithValidationErrors()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);

        using var client = factory.CreateClient();
        await ValidateTestDatabaseConnectionStringAsync(factory);
        using var response = await client.PostAsJsonAsync("/api/students", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("FirstName", out _));
        Assert.True(errors.TryGetProperty("LastName", out _));
        Assert.True(errors.TryGetProperty("DateOfBirth", out _));
        Assert.True(errors.TryGetProperty("EnrollmentDate", out _));
        Assert.True(errors.TryGetProperty("IsActive", out _));
    }

    [Fact]
    public async Task Create_WhenEmailIsInvalid_ReturnsBadRequest()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);

        var request = CreateValidRequest("not-an-email-address");
        using var client = factory.CreateClient();
        await ValidateTestDatabaseConnectionStringAsync(factory);
        using var response = await client.PostAsJsonAsync("/api/students", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Email", out _));
    }

    [Fact]
    public async Task Create_WhenEmailAlreadyExists_ReturnsConflictWithoutDatabaseDetails()
    {
        using var factory = new SchoolManagementApiFactory();
        await TestDatabaseInitializer.InitializeAsync(factory);

        var email = CreateUniqueEmail("duplicate");
        var request = CreateValidRequest(email);

        try
        {
            using var client = factory.CreateClient();
            await ValidateTestDatabaseConnectionStringAsync(factory);
            using var firstResponse = await client.PostAsJsonAsync("/api/students", request);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            await ValidateTestDatabaseConnectionStringAsync(factory);
            using var duplicateResponse = await client.PostAsJsonAsync("/api/students", request);

            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

            var responseBody = await duplicateResponse.Content.ReadAsStringAsync();
            Assert.Contains("already exists", responseBody, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("SqlException", responseBody, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("DbUpdateException", responseBody, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("IX_Students_Email", responseBody, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("duplicate key", responseBody, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await CleanupStudentByEmailAsync(factory, email);
        }
    }

    private static StudentCreateRequest CreateValidRequest(string email)
    {
        return new StudentCreateRequest
        {
            FirstName = "Lena",
            LastName = "Thompson",
            Email = email,
            DateOfBirth = new DateTime(2008, 6, 12),
            EnrollmentDate = new DateTime(2025, 9, 1),
            IsActive = true
        };
    }

    private static string CreateUniqueEmail(string scenario)
    {
        return $"milestone4b6.{scenario}.{Guid.NewGuid():N}@example.com";
    }

    private static async Task ValidateTestDatabaseConnectionStringAsync(
        SchoolManagementApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
        TestDatabaseSafety.EnsureSafeConnectionString(
            dbContext.Database.GetDbConnection().ConnectionString);
    }

    private static async Task CleanupStudentByEmailAsync(
        SchoolManagementApiFactory factory,
        string email)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolManagementDbContext>();
        var student = await dbContext.Students
            .SingleOrDefaultAsync(candidate => candidate.Email == email);

        if (student is null)
        {
            return;
        }

        dbContext.Students.Remove(student);
        TestDatabaseSafety.EnsureSafeConnectionString(
            dbContext.Database.GetDbConnection().ConnectionString);
        await dbContext.SaveChangesAsync();
    }
}
