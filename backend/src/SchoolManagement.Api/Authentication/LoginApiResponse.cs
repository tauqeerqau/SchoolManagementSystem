namespace SchoolManagement.Api.Authentication;

public sealed record LoginApiResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
