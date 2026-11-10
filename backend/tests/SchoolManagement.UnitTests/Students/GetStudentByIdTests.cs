using SchoolManagement.Application.Students;
using SchoolManagement.Domain;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Students;

public sealed class GetStudentByIdTests
{
    [Fact]
    public async Task ExecuteAsync_WhenStudentExists_ReturnsMappedStudentDto()
    {
        var dateOfBirth = new DateTime(2004, 11, 8);
        var enrollmentDate = new DateTime(2023, 9, 4);
        const int requestedStudentId = 21;
        var student = new Student
        {
            StudentId = requestedStudentId,
            FirstName = "Mia",
            LastName = "Chen",
            Email = "mia.chen@example.com",
            DateOfBirth = dateOfBirth,
            EnrollmentDate = enrollmentDate,
            IsActive = true
        };
        var studentReadStore = new FakeStudentReadStore([student]);
        var studentService = FakeStudentServiceFactory.Create(readStore: studentReadStore);

        var result = await studentService.GetByIdAsync(
            requestedStudentId,
            CancellationToken.None);

        Assert.Equal(
            new StudentDto(
                requestedStudentId,
                "Mia",
                "Chen",
                "mia.chen@example.com",
                dateOfBirth,
                enrollmentDate,
                true),
            result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        var studentReadStore = new FakeStudentReadStore([]);
        var studentService = FakeStudentServiceFactory.Create(readStore: studentReadStore);

        var result = await studentService.GetByIdAsync(
            999,
            CancellationToken.None);

        Assert.Null(result);
    }
}
