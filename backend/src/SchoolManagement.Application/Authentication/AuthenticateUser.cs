namespace SchoolManagement.Application.Authentication;

public sealed class AuthenticateUser(IUserAuthenticator userAuthenticator)
{
    public async Task<LoginResult> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userAuthenticator.AuthenticateAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return response is null
            ? LoginResult.InvalidCredentials()
            : LoginResult.Succeeded(response);
    }
}
