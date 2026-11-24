namespace SchoolManagement.Api.Authentication;

public interface IAccessTokenGenerator
{
    GeneratedAccessToken Generate(
        string userId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> roles);
}
