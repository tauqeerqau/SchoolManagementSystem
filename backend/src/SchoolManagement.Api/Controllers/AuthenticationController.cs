using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SchoolManagement.Api.Authentication;
using SchoolManagement.Application.Authentication;

namespace SchoolManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    RegisterStudent registerStudent,
    AuthenticateUser authenticateUser,
    IAccessTokenGenerator accessTokenGenerator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticateUser.ExecuteAsync(request, cancellationToken);
        if (result.Status == LoginStatus.InvalidCredentials || result.Response is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials.",
                Detail = "Email or password is incorrect."
            });
        }

        var user = result.Response;
        var token = accessTokenGenerator.Generate(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles);

        return Ok(new LoginApiResponse(
            token.Token,
            token.ExpiresAtUtc,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<StudentRegistrationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        StudentRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await registerStudent.ExecuteAsync(request, cancellationToken);

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
