using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain;

namespace SchoolManagement.Infrastructure.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(enrollment => enrollment.EnrollmentId);

        builder.Property(enrollment => enrollment.AcademicYear)
            .IsRequired();

        builder.Property(enrollment => enrollment.EnrolledOn)
            .IsRequired();

        builder.HasOne(enrollment => enrollment.Student)
            .WithMany(student => student.Enrollments)
            .HasForeignKey(enrollment => enrollment.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.Course)
            .WithMany(course => course.Enrollments)
            .HasForeignKey(enrollment => enrollment.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(enrollment => new
            {
                enrollment.StudentId,
                enrollment.CourseId,
                enrollment.AcademicYear
            })
            .IsUnique();

        // EF creates this course-first FK index by convention. Declaring it here
        // documents that query path without adding a duplicate index.
        builder.HasIndex(enrollment => enrollment.CourseId);
    }
}
