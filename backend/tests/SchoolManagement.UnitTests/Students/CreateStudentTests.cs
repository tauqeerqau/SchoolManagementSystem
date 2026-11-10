using SchoolManagement.Application.Students;
using SchoolManagement.Domain;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Students;

public sealed class CreateStudentTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesStudentAndReturnsMappedDto()
    {
        const int generatedStudentId = 37;
        var dateOfBirth = new DateTime(2008, 5, 19);
        var enrollmentDate = new DateTime(2025, 9, 2);
        var request = new StudentCreateRequest
        {
            FirstName = "Isabella",
            LastName = "Morgan",
            Email = "isabella.morgan@example.com",
            DateOfBirth = dateOfBirth,
            EnrollmentDate = enrollmentDate,
            IsActive = true
        };
        var studentCreateStore = new FakeStudentCreateStore(generatedStudentId);
        var studentService = FakeStudentServiceFactory.Create(createStore: studentCreateStore);

        var result = await studentService.CreateAsync(
            request,
            CancellationToken.None);

        Assert.True(studentCreateStore.CreateCalled);

        var capturedStudent = Assert.IsType<Student>(studentCreateStore.CreatedStudent);
        Assert.Equal("Isabella", capturedStudent.FirstName);
        Assert.Equal("Morgan", capturedStudent.LastName);
        Assert.Equal("isabella.morgan@example.com", capturedStudent.Email);
        Assert.Equal(dateOfBirth, capturedStudent.DateOfBirth);
        Assert.Equal(enrollmentDate, capturedStudent.EnrollmentDate);
        Assert.True(capturedStudent.IsActive);

        Assert.Equal(
            new StudentDto(
                generatedStudentId,
                "Isabella",
                "Morgan",
                "isabella.morgan@example.com",
                dateOfBirth,
                enrollmentDate,
                true),
            result);
    }
}
