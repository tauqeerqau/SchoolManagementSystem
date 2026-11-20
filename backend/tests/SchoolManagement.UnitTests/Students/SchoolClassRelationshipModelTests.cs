using Microsoft.EntityFrameworkCore;
using SchoolManagement.Domain;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.UnitTests.Students;

public sealed class SchoolClassRelationshipModelTests
{
    [Fact]
    public void StudentHasOptionalManyToOneRelationshipToSchoolClassWithRestrictedDelete()
    {
        var options = new DbContextOptionsBuilder<SchoolManagementDbContext>()
            .UseSqlServer("Server=(local);Database=ModelOnly;")
            .Options;
        using var dbContext = new SchoolManagementDbContext(options);

        var studentType = dbContext.Model.FindEntityType(typeof(Student));
        var schoolClassType = dbContext.Model.FindEntityType(typeof(SchoolClass));

        Assert.NotNull(studentType);
        Assert.NotNull(schoolClassType);

        var relationship = Assert.Single(studentType.GetForeignKeys());
        Assert.Equal(schoolClassType, relationship.PrincipalEntityType);
        Assert.Equal("ClassId", Assert.Single(relationship.Properties).Name);
        Assert.False(relationship.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, relationship.DeleteBehavior);
        Assert.Equal("SchoolClass", relationship.DependentToPrincipal?.Name);
        Assert.Equal("Students", relationship.PrincipalToDependent?.Name);
    }
}
