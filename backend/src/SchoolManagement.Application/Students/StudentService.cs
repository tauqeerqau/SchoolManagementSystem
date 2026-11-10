using SchoolManagement.Domain;

namespace SchoolManagement.Application.Students;

public sealed class StudentService(
    IStudentStore studentStore) : IStudentService
{
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var students = await studentStore.GetAllAsync(cancellationToken);
        return students.Select(StudentDto.FromStudent).ToArray();
    }

    public async Task<StudentDto?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken)
    {
        var student = await studentStore.GetByIdAsync(studentId, cancellationToken);
        return student is null ? null : StudentDto.FromStudent(student);
    }

    public async Task<StudentDto> CreateAsync(
        StudentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var student = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth!.Value,
            EnrollmentDate = request.EnrollmentDate!.Value,
            IsActive = request.IsActive!.Value
        };

        var createdStudent = await studentStore.CreateAsync(student, cancellationToken);
        return StudentDto.FromStudent(createdStudent);
    }

    public async Task<StudentDto?> UpdateAsync(
        int studentId,
        StudentUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var updatedValues = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth!.Value,
            EnrollmentDate = request.EnrollmentDate!.Value,
            IsActive = request.IsActive!.Value
        };

        var student = await studentStore.UpdateAsync(
            studentId,
            updatedValues,
            cancellationToken);

        return student is null ? null : StudentDto.FromStudent(student);
    }

    public Task<bool> DeleteAsync(int studentId, CancellationToken cancellationToken) =>
        studentStore.DeleteAsync(studentId, cancellationToken);
}
