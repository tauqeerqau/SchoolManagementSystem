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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SchoolManagementDbContext).Assembly);
    }
}
