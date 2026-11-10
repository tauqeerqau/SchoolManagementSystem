namespace SchoolManagement.Infrastructure.Identity;

public static class IdentityRoleNames
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static IReadOnlyList<string> All { get; } = [Admin, Teacher, Student];
}
