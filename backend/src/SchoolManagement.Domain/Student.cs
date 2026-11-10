namespace SchoolManagement.Domain;

public class Student
{
    public int StudentId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public bool IsActive { get; set; }
}
