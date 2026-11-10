using SchoolManagement.Application.Students;
using SchoolManagement.Domain;

namespace SchoolManagement.UnitTests.Fakes;

internal sealed class FakeStudentReadStore(IReadOnlyList<Student> students)
    
{
    public Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult(students);
    }

    public Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken)
    {
        var student = students.FirstOrDefault(item => item.StudentId == studentId);
        return Task.FromResult(student);
    }
}
