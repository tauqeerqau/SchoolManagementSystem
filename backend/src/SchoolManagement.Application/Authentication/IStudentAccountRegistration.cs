namespace SchoolManagement.Application.Authentication;

public interface IStudentAccountRegistration
{
    Task<StudentAccountRegistrationResult> CreateStudentAccountAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken);
}
