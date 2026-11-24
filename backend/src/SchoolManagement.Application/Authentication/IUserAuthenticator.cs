namespace SchoolManagement.Application.Authentication;

public interface IUserAuthenticator
{
    Task<LoginResponse?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
