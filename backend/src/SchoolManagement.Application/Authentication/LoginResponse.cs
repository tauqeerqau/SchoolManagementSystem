namespace SchoolManagement.Application.Authentication;

public sealed record LoginResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
