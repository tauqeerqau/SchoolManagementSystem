using SchoolManagement.Application.Students;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Students;

public sealed class DeleteStudentTests
{
    [Fact]
    public async Task ExecuteAsync_WhenStudentExists_ReturnsTrueAndPassesStudentId()
    {
        const int studentId = 42;
        var studentDeleteStore = new FakeStudentDeleteStore(studentExists: true);
        var studentService = FakeStudentServiceFactory.Create(deleteStore: studentDeleteStore);

        var result = await studentService.DeleteAsync(
            studentId,
            CancellationToken.None);

        Assert.True(result);
        Assert.True(studentDeleteStore.DeleteCalled);
        Assert.Equal(studentId, studentDeleteStore.DeletedStudentId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentDoesNotExist_ReturnsFalse()
    {
        var studentDeleteStore = new FakeStudentDeleteStore(studentExists: false);
        var studentService = FakeStudentServiceFactory.Create(deleteStore: studentDeleteStore);

        var result = await studentService.DeleteAsync(
            999,
            CancellationToken.None);

        Assert.False(result);
    }
}
