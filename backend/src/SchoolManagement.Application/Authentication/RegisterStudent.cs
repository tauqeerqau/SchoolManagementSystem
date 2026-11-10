namespace SchoolManagement.Application.Authentication;

public sealed class RegisterStudent(IStudentAccountRegistration accountRegistration)
{
    public async Task<StudentRegistrationResult> ExecuteAsync(
        StudentRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return StudentRegistrationResult.ValidationFailed(
                new Dictionary<string, string[]>
                {
                    [nameof(request.ConfirmPassword)] = ["The passwords do not match."]
                });
        }

        cancellationToken.ThrowIfCancellationRequested();

        var accountResult = await accountRegistration.CreateStudentAccountAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            cancellationToken);

        return accountResult.Status switch
        {
            StudentAccountRegistrationStatus.Created => StudentRegistrationResult.Succeeded(
                new StudentRegistrationResponse(
                    accountResult.UserId!,
                    request.FirstName,
                    request.LastName,
                    request.Email)),
            StudentAccountRegistrationStatus.DuplicateEmail =>
                StudentRegistrationResult.DuplicateEmail(),
            StudentAccountRegistrationStatus.ValidationFailed =>
                StudentRegistrationResult.ValidationFailed(accountResult.ValidationErrors),
            _ => StudentRegistrationResult.Failed()
        };
    }
}
