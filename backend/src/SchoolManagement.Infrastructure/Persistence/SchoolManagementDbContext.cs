using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SchoolManagement.Domain;
using SchoolManagement.Infrastructure.Identity;

namespace SchoolManagement.Infrastructure.Persistence;

public sealed class SchoolManagementDbContext(
    DbContextOptions<SchoolManagementDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<SchoolClass> Classes => Set<SchoolClass>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<TeacherCourseAssignment> TeacherCourseAssignments =>
        Set<TeacherCourseAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SchoolManagementDbContext).Assembly);
    }
}
