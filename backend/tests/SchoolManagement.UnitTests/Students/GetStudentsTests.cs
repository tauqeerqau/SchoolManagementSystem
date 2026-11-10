using SchoolManagement.Application.Students;
using SchoolManagement.Domain;
using SchoolManagement.UnitTests.Fakes;

namespace SchoolManagement.UnitTests.Students;

public sealed class GetStudentsTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsMappedStudentDtos()
    {
        var firstDateOfBirth = new DateTime(2005, 3, 14);
        var firstEnrollmentDate = new DateTime(2024, 9, 1);
        var secondDateOfBirth = new DateTime(2006, 7, 22);
        var secondEnrollmentDate = new DateTime(2025, 1, 15);
        var students = new List<Student>
        {
            new()
            {
                StudentId = 11,
                FirstName = "Ava",
                LastName = "Patel",
                Email = "ava.patel@example.com",
                DateOfBirth = firstDateOfBirth,
                EnrollmentDate = firstEnrollmentDate,
                IsActive = true
            },
            new()
            {
                StudentId = 12,
                FirstName = "Noah",
                LastName = "Wilson",
                Email = "noah.wilson@example.com",
                DateOfBirth = secondDateOfBirth,
                EnrollmentDate = secondEnrollmentDate,
                IsActive = false
            }
        };

        var studentReadStore = new FakeStudentReadStore(students);
        var studentService = FakeStudentServiceFactory.Create(readStore: studentReadStore);

        var result = await studentService.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(
            new StudentDto(
                11,
                "Ava",
                "Patel",
                "ava.patel@example.com",
                firstDateOfBirth,
                firstEnrollmentDate,
                true),
            result[0]);
        Assert.Equal(
            new StudentDto(
                12,
                "Noah",
                "Wilson",
                "noah.wilson@example.com",
                secondDateOfBirth,
                secondEnrollmentDate,
                false),
            result[1]);
    }

}
