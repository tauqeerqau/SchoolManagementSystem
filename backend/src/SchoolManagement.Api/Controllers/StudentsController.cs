using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Students;

namespace SchoolManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(
    IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var students = await studentService.GetAllAsync(cancellationToken);
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid student ID.",
                Detail = "The ID must be a positive integer."
            });
        }

        var student = await studentService.GetByIdAsync(id, cancellationToken);

        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(
        StudentCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await studentService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = student.StudentId },
                student);
        }
        catch (DuplicateStudentEmailException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already in use.",
                Detail = "A student with this email address already exists."
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentDto>> Update(
        int id,
        StudentUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid student ID.",
                Detail = "The ID must be a positive integer."
            });
        }

        try
        {
            var student = await studentService.UpdateAsync(
                id,
                request,
                cancellationToken);

            return student is null ? NotFound() : Ok(student);
        }
        catch (DuplicateStudentEmailException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already in use.",
                Detail = "A student with this email address already exists."
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid student ID.",
                Detail = "The ID must be a positive integer."
            });
        }

        var deleted = await studentService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
