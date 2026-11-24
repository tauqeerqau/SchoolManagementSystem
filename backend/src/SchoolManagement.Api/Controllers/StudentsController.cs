using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Application.Students;

namespace SchoolManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(
    IStudentService studentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var students = await studentService.GetAllAsync(cancellationToken);
        return Ok(students);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StudentDto>> Create(
        StudentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var student = await studentService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = student.StudentId },
            student);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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

        var student = await studentService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return student is null ? NotFound() : Ok(student);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
