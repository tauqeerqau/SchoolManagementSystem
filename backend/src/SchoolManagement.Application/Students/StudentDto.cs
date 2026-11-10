using SchoolManagement.Domain;

namespace SchoolManagement.Application.Students;

public sealed record StudentDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth,
    DateTime EnrollmentDate,
    bool IsActive)
{
    public static StudentDto FromStudent(Student student) => new(
        student.StudentId,
        student.FirstName,
        student.LastName,
        student.Email,
        student.DateOfBirth,
        student.EnrollmentDate,
        student.IsActive);
}
