namespace SchoolManagement.Application.Authentication;

public enum LoginStatus
{
    Succeeded,
    InvalidCredentials
}

public sealed record LoginResult(LoginStatus Status, LoginResponse? Response)
{
    public static LoginResult Succeeded(LoginResponse response) =>
        new(LoginStatus.Succeeded, response);

    public static LoginResult InvalidCredentials() =>
        new(LoginStatus.InvalidCredentials, null);
}
