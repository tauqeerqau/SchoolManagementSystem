namespace SchoolManagement.Application.Authentication;

public enum StudentAccountRegistrationStatus
{
    Created,
    DuplicateEmail,
    ValidationFailed,
    Failed
}

public sealed record StudentAccountRegistrationResult(
    StudentAccountRegistrationStatus Status,
    string? UserId,
    IReadOnlyDictionary<string, string[]> ValidationErrors)
{
    public static StudentAccountRegistrationResult Created(string userId) =>
        new(StudentAccountRegistrationStatus.Created, userId, EmptyErrors);

    public static StudentAccountRegistrationResult DuplicateEmail() =>
        new(StudentAccountRegistrationStatus.DuplicateEmail, null, EmptyErrors);

    public static StudentAccountRegistrationResult ValidationFailed(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(StudentAccountRegistrationStatus.ValidationFailed, null, errors);

    public static StudentAccountRegistrationResult Failed() =>
        new(StudentAccountRegistrationStatus.Failed, null, EmptyErrors);

    private static IReadOnlyDictionary<string, string[]> EmptyErrors { get; } =
        new Dictionary<string, string[]>();
}
