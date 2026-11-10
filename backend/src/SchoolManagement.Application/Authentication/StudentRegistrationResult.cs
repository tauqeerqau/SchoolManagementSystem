namespace SchoolManagement.Application.Authentication;

public enum StudentRegistrationStatus
{
    Succeeded,
    DuplicateEmail,
    ValidationFailed,
    Failed
}

public sealed record StudentRegistrationResult(
    StudentRegistrationStatus Status,
    StudentRegistrationResponse? Response,
    IReadOnlyDictionary<string, string[]> ValidationErrors)
{
    public static StudentRegistrationResult Succeeded(
        StudentRegistrationResponse response) =>
        new(StudentRegistrationStatus.Succeeded, response, EmptyErrors);

    public static StudentRegistrationResult DuplicateEmail() =>
        new(StudentRegistrationStatus.DuplicateEmail, null, EmptyErrors);

    public static StudentRegistrationResult ValidationFailed(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(StudentRegistrationStatus.ValidationFailed, null, errors);

    public static StudentRegistrationResult Failed() =>
        new(StudentRegistrationStatus.Failed, null, EmptyErrors);

    private static IReadOnlyDictionary<string, string[]> EmptyErrors { get; } =
        new Dictionary<string, string[]>();
}
