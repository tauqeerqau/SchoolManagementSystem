using Microsoft.EntityFrameworkCore;
using SchoolManagement.Domain;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.UnitTests.Students;

public sealed class CourseEnrollmentRelationshipModelTests
{
    [Fact]
    public void EnrollmentRelationshipsAndIndexesAreConfigured()
    {
        var options = new DbContextOptionsBuilder<SchoolManagementDbContext>()
            .UseSqlServer("Server=(local);Database=ModelOnly;")
            .Options;
        using var dbContext = new SchoolManagementDbContext(options);

        var studentType = dbContext.Model.FindEntityType(typeof(Student));
        var courseType = dbContext.Model.FindEntityType(typeof(Course));
        var enrollmentType = dbContext.Model.FindEntityType(typeof(Enrollment));

        Assert.NotNull(studentType);
        Assert.NotNull(courseType);
        Assert.NotNull(enrollmentType);

        var studentRelationship = Assert.Single(
            enrollmentType.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType == studentType);
        Assert.Equal("StudentId", Assert.Single(studentRelationship.Properties).Name);
        Assert.True(studentRelationship.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, studentRelationship.DeleteBehavior);
        Assert.Equal("Student", studentRelationship.DependentToPrincipal?.Name);
        Assert.Equal("Enrollments", studentRelationship.PrincipalToDependent?.Name);

        var courseRelationship = Assert.Single(
            enrollmentType.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType == courseType);
        Assert.Equal("CourseId", Assert.Single(courseRelationship.Properties).Name);
        Assert.True(courseRelationship.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, courseRelationship.DeleteBehavior);
        Assert.Equal("Course", courseRelationship.DependentToPrincipal?.Name);
        Assert.Equal("Enrollments", courseRelationship.PrincipalToDependent?.Name);

        var uniqueEnrollmentIndex = Assert.Single(
            enrollmentType.GetIndexes(),
            index => index.IsUnique);
        Assert.Equal(
            new[] { "StudentId", "CourseId", "AcademicYear" },
            uniqueEnrollmentIndex.Properties.Select(property => property.Name));

        var courseFirstIndex = Assert.Single(
            enrollmentType.GetIndexes(),
            index => index.Properties.Count == 1
                && index.Properties[0].Name == "CourseId");
        Assert.False(courseFirstIndex.IsUnique);

        Assert.DoesNotContain(
            enrollmentType.GetIndexes(),
            index => index.Properties.Count == 1
                && index.Properties[0].Name == "StudentId");
    }
}
