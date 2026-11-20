using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain;

namespace SchoolManagement.Infrastructure.Persistence.Configurations;

public sealed class TeacherCourseAssignmentConfiguration
    : IEntityTypeConfiguration<TeacherCourseAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherCourseAssignment> builder)
    {
        builder.ToTable("TeacherCourseAssignments");

        builder.HasKey(assignment => assignment.TeacherCourseAssignmentId);

        builder.Property(assignment => assignment.AcademicYear)
            .IsRequired();

        builder.HasOne(assignment => assignment.Teacher)
            .WithMany(teacher => teacher.Assignments)
            .HasForeignKey(assignment => assignment.TeacherId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Course)
            .WithMany(course => course.Assignments)
            .HasForeignKey(assignment => assignment.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(assignment => new
            {
                assignment.TeacherId,
                assignment.CourseId,
                assignment.AcademicYear
            })
            .IsUnique();

        builder.HasIndex(assignment => new
        {
            assignment.CourseId,
            assignment.AcademicYear
        });
    }
}
