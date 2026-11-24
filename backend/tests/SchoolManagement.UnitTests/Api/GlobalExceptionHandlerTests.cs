using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Api.ExceptionHandling;
using SchoolManagement.Application.Students;

namespace SchoolManagement.UnitTests.Api;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public Task TryHandleAsync_WhenStudentEmailIsDuplicate_ReturnsConflictProblemDetails() =>
        AssertProblemDetailsAsync(
            new DuplicateStudentEmailException(),
            StatusCodes.Status409Conflict,
            "Email already in use.",
            "A student with this email address already exists.");

    [Fact]
    public Task TryHandleAsync_WhenExceptionIsUnexpected_ReturnsGenericServerErrorProblemDetails() =>
        AssertProblemDetailsAsync(
            new InvalidOperationException("sensitive internal detail"),
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            "The server encountered an unexpected error.");

    private static async Task AssertProblemDetailsAsync(
        Exception exception,
        int expectedStatus,
        string expectedTitle,
        string expectedDetail)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        await using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Request.Method = "POST";
        context.Request.Path = "/api/students";
        context.TraceIdentifier = "test-trace-id";
        context.Response.Body = new MemoryStream();

        var handler = ActivatorUtilities.CreateInstance<GlobalExceptionHandler>(provider);
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;
        Assert.Equal(expectedStatus, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedTitle, root.GetProperty("title").GetString());
        Assert.Equal(expectedDetail, root.GetProperty("detail").GetString());
        Assert.Equal("test-trace-id", root.GetProperty("traceId").GetString());

        if (expectedStatus == StatusCodes.Status500InternalServerError)
        {
            Assert.DoesNotContain("sensitive internal detail", root.GetRawText());
        }
    }
}
