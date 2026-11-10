namespace SchoolManagement.Application.Authentication;

public sealed record StudentRegistrationResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Email);
