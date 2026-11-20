using Microsoft.EntityFrameworkCore;
using SchoolManagement.Domain;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.UnitTests.Students;

public sealed class TeacherCourseAssignmentRelationshipModelTests
{
    [Fact]
    public void TeacherCourseAssignmentRelationshipsAndIndexesAreConfigured()
    {
        var options = new DbContextOptionsBuilder<SchoolManagementDbContext>()
            .UseSqlServer("Server=(local);Database=ModelOnly;")
            .Options;
        using var dbContext = new SchoolManagementDbContext(options);

        var teacherType = dbContext.Model.FindEntityType(typeof(Teacher));
        var courseType = dbContext.Model.FindEntityType(typeof(Course));
        var assignmentType = dbContext.Model.FindEntityType(typeof(TeacherCourseAssignment));

        Assert.NotNull(teacherType);
        Assert.NotNull(courseType);
        Assert.NotNull(assignmentType);

        var teacherRelationship = Assert.Single(
            assignmentType.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType == teacherType);
        Assert.Equal("TeacherId", Assert.Single(teacherRelationship.Properties).Name);
        Assert.True(teacherRelationship.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, teacherRelationship.DeleteBehavior);
        Assert.Equal("Teacher", teacherRelationship.DependentToPrincipal?.Name);
        Assert.Equal("Assignments", teacherRelationship.PrincipalToDependent?.Name);

        var courseRelationship = Assert.Single(
            assignmentType.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType == courseType);
        Assert.Equal("CourseId", Assert.Single(courseRelationship.Properties).Name);
        Assert.True(courseRelationship.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, courseRelationship.DeleteBehavior);
        Assert.Equal("Course", courseRelationship.DependentToPrincipal?.Name);
        Assert.Equal("Assignments", courseRelationship.PrincipalToDependent?.Name);

        var uniqueAssignmentIndex = Assert.Single(
            assignmentType.GetIndexes(),
            index => index.IsUnique);
        Assert.Equal(
            new[] { "TeacherId", "CourseId", "AcademicYear" },
            uniqueAssignmentIndex.Properties.Select(property => property.Name));

        var courseYearIndex = Assert.Single(
            assignmentType.GetIndexes(),
            index => !index.IsUnique);
        Assert.Equal(
            new[] { "CourseId", "AcademicYear" },
            courseYearIndex.Properties.Select(property => property.Name));

        Assert.Equal(2, assignmentType.GetIndexes().Count());
        Assert.DoesNotContain(
            assignmentType.GetIndexes(),
            index => index.Properties.Count == 1
                && index.Properties[0].Name is "TeacherId" or "CourseId");
    }
}
