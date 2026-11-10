namespace SchoolManagement.Application.Students;

public sealed class DuplicateStudentEmailException : Exception
{
    public DuplicateStudentEmailException()
        : base("A student with this email address already exists.")
    {
    }
}
