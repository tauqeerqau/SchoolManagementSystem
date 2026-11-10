using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Application.Authentication;
using SchoolManagement.Application.Students;
using SchoolManagement.Infrastructure.Persistence;
using SchoolManagement.Infrastructure.Identity;

namespace SchoolManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IStudentStore, StudentStore>();
        services.AddScoped<IStudentService, StudentService>();

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SchoolManagementDbContext>();
        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
        });

        services.AddScoped<IStudentAccountRegistration, StudentAccountRegistration>();
        services.AddScoped<IIdentityRoleProvisioner, IdentityRoleProvisioner>();

        services.AddDbContext<SchoolManagementDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("SchoolManagementDatabase");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'SchoolManagementDatabase' was not found.");
            }

            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
