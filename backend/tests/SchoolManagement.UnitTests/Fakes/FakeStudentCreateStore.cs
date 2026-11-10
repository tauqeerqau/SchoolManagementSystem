using SchoolManagement.Application.Students;
using SchoolManagement.Domain;

namespace SchoolManagement.UnitTests.Fakes;

internal sealed class FakeStudentCreateStore(int generatedStudentId)
{
    public bool CreateCalled { get; private set; }

    public Student? CreatedStudent { get; private set; }

    public Task<Student> CreateAsync(
        Student student,
        CancellationToken cancellationToken)
    {
        CreateCalled = true;
        CreatedStudent = student;
        student.StudentId = generatedStudentId;

        return Task.FromResult(student);
    }
}
