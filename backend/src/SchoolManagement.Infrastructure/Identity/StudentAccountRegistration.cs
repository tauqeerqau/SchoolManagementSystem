using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Authentication;

namespace SchoolManagement.Infrastructure.Identity;

public sealed class StudentAccountRegistration(
    UserManager<ApplicationUser> userManager) : IStudentAccountRegistration
{
    public async Task<StudentAccountRegistrationResult> CreateStudentAccountAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = email,
            Email = email
        };

        IdentityResult createResult;
        try
        {
            createResult = await userManager.CreateAsync(user, password);
        }
        catch (DbUpdateException exception) when (IsDuplicateEmailViolation(exception))
        {
            return StudentAccountRegistrationResult.DuplicateEmail();
        }

        if (!createResult.Succeeded)
        {
            return MapIdentityErrors(createResult.Errors);
        }

        IdentityResult roleResult;
        try
        {
            roleResult = await userManager.AddToRoleAsync(user, IdentityRoleNames.Student);
        }
        catch (Exception)
        {
            await RemovePartiallyCreatedUserAsync(user);
            return StudentAccountRegistrationResult.Failed();
        }

        if (!roleResult.Succeeded)
        {
            await RemovePartiallyCreatedUserAsync(user);
            return StudentAccountRegistrationResult.Failed();
        }

        return StudentAccountRegistrationResult.Created(user.Id);
    }

    private async Task RemovePartiallyCreatedUserAsync(ApplicationUser user)
    {
        var cleanupResult = await userManager.DeleteAsync(user);
        if (!cleanupResult.Succeeded)
        {
            throw new InvalidOperationException("Student account provisioning failed.");
        }
    }

    private static StudentAccountRegistrationResult MapIdentityErrors(
        IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.ToArray();

        if (errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            return StudentAccountRegistrationResult.DuplicateEmail();
        }

        var validationErrors = new Dictionary<string, List<string>>();
        foreach (var error in errors)
        {
            if (error.Code == "InvalidEmail")
            {
                AddValidationError(validationErrors, "Email", "Enter a valid email address.");
            }
            else if (error.Code.StartsWith("Password", StringComparison.Ordinal))
            {
                AddValidationError(
                    validationErrors,
                    "Password",
                    "The password does not meet the security requirements.");
            }
        }

        if (validationErrors.Count > 0)
        {
            return StudentAccountRegistrationResult.ValidationFailed(
                validationErrors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Distinct().ToArray()));
        }

        return StudentAccountRegistrationResult.Failed();
    }

    private static void AddValidationError(
        IDictionary<string, List<string>> errors,
        string key,
        string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }

    private static bool IsDuplicateEmailViolation(DbUpdateException exception)
    {
        for (Exception? current = exception;
             current is not null;
             current = current.InnerException)
        {
            if (current is SqlException sqlException
                && sqlException.Number is 2601 or 2627
                && sqlException.Message.Contains("EmailIndex", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
