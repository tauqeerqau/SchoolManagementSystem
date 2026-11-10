using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Authentication;

namespace SchoolManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    RegisterStudent registerStudent,
    ILogger<AuthenticationController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<StudentRegistrationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        StudentRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        StudentRegistrationResult result;
        try
        {
            result = await registerStudent.ExecuteAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogError("Student account registration failed unexpectedly.");
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Registration could not be completed.",
                detail: "An unexpected error occurred.");
        }

        return result.Status switch
        {
            StudentRegistrationStatus.Succeeded =>
                StatusCode(StatusCodes.Status201Created, result.Response),
            StudentRegistrationStatus.DuplicateEmail => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration could not be completed.",
                Detail = "Review the submitted information and try again."
            }),
            StudentRegistrationStatus.ValidationFailed => BadRequest(
                CreateValidationProblem(result.ValidationErrors)),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Registration could not be completed.",
                detail: "An unexpected error occurred.")
        };
    }

    private static ValidationProblemDetails CreateValidationProblem(
        IReadOnlyDictionary<string, string[]> validationErrors)
    {
        var problem = new ValidationProblemDetails
        {
            Title = "Registration validation failed."
        };

        foreach (var error in validationErrors)
        {
            problem.Errors[error.Key] = error.Value;
        }

        return problem;
    }
}
