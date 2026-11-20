using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain;

namespace SchoolManagement.Infrastructure.Persistence.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(teacher => teacher.TeacherId);

        builder.Property(teacher => teacher.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
