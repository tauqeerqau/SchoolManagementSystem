using SchoolManagement.Application.Students;
using SchoolManagement.Domain;

namespace SchoolManagement.UnitTests.Fakes;

internal sealed class FakeStudentUpdateStore(bool studentFound)
{
    public bool StudentFound { get; set; } = studentFound;

    public bool UpdateCalled { get; private set; }

    public int? StudentIdReceived { get; private set; }

    public Student? UpdatedStudent { get; private set; }

    public Task<Student?> UpdateAsync(
        int studentId,
        Student updatedValues,
        CancellationToken cancellationToken)
    {
        UpdateCalled = true;
        StudentIdReceived = studentId;
        UpdatedStudent = updatedValues;

        if (!StudentFound)
        {
            return Task.FromResult<Student?>(null);
        }

        updatedValues.StudentId = studentId;
        return Task.FromResult<Student?>(updatedValues);
    }
}
