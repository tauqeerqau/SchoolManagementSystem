using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Domain;

namespace SchoolManagement.Infrastructure.Persistence.Configurations;

public sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(schoolClass => schoolClass.ClassId);

        builder.Property(schoolClass => schoolClass.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
