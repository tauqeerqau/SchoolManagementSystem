using SchoolManagement.Application.Students;
using SchoolManagement.Domain;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Students;

public sealed class UpdateStudentTests
{
    [Fact]
    public async Task ExecuteAsync_WhenStudentExists_UpdatesStudentAndReturnsMappedDto()
    {
        const int studentId = 42;
        var dateOfBirth = new DateTime(2007, 2, 18);
        var enrollmentDate = new DateTime(2024, 9, 3);
        var request = new StudentUpdateRequest
        {
            FirstName = "Ethan",
            LastName = "Bennett",
            Email = "ethan.bennett@example.com",
            DateOfBirth = dateOfBirth,
            EnrollmentDate = enrollmentDate,
            IsActive = true
        };
        var studentUpdateStore = new FakeStudentUpdateStore(studentFound: true);
        var studentService = FakeStudentServiceFactory.Create(updateStore: studentUpdateStore);

        var result = await studentService.UpdateAsync(
            studentId,
            request,
            CancellationToken.None);

        Assert.True(studentUpdateStore.UpdateCalled);
        Assert.Equal(studentId, studentUpdateStore.StudentIdReceived);

        var capturedStudent = Assert.IsType<Student>(studentUpdateStore.UpdatedStudent);
        Assert.Equal("Ethan", capturedStudent.FirstName);
        Assert.Equal("Bennett", capturedStudent.LastName);
        Assert.Equal("ethan.bennett@example.com", capturedStudent.Email);
        Assert.Equal(dateOfBirth, capturedStudent.DateOfBirth);
        Assert.Equal(enrollmentDate, capturedStudent.EnrollmentDate);
        Assert.True(capturedStudent.IsActive);

        Assert.Equal(
            new StudentDto(
                studentId,
                "Ethan",
                "Bennett",
                "ethan.bennett@example.com",
                dateOfBirth,
                enrollmentDate,
                true),
            result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentDoesNotExist_ReturnsNull()
    {
        var studentUpdateStore = new FakeStudentUpdateStore(studentFound: false);
        var studentService = FakeStudentServiceFactory.Create(updateStore: studentUpdateStore);

        var result = await studentService.UpdateAsync(
            999,
            new StudentUpdateRequest
            {
                FirstName = "Ethan",
                LastName = "Bennett",
                Email = "ethan.bennett@example.com",
                DateOfBirth = new DateTime(2007, 2, 18),
                EnrollmentDate = new DateTime(2024, 9, 3),
                IsActive = true
            },
            CancellationToken.None);

        Assert.Null(result);
    }
}
