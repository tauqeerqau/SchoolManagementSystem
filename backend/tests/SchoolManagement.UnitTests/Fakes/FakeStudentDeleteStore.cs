using SchoolManagement.Application.Students;

namespace SchoolManagement.UnitTests.Fakes;

internal sealed class FakeStudentDeleteStore(bool studentExists)
{
    public bool StudentExists { get; set; } = studentExists;

    public bool DeleteCalled { get; private set; }

    public int? DeletedStudentId { get; private set; }

    public Task<bool> DeleteAsync(
        int studentId,
        CancellationToken cancellationToken)
    {
        DeleteCalled = true;
        DeletedStudentId = studentId;

        return Task.FromResult(StudentExists);
    }
}
