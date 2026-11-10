using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain;

namespace SchoolManagement.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(student => student.StudentId);

        builder.Property(student => student.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(student => student.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(student => student.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(student => student.Email)
            .IsUnique();

        builder.Property(student => student.DateOfBirth)
            .IsRequired();

        builder.Property(student => student.EnrollmentDate)
            .IsRequired();

        builder.Property(student => student.IsActive)
            .IsRequired();
    }
}
