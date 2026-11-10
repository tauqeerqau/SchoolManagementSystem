using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Application.Students;

public sealed class StudentUpdateRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public DateTime? DateOfBirth { get; init; }

    [Required]
    public DateTime? EnrollmentDate { get; init; }

    [Required]
    public bool? IsActive { get; init; }
}
