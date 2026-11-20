namespace SchoolManagement.Domain;

public class Teacher
{
    public int TeacherId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<TeacherCourseAssignment> Assignments { get; set; } =
        new List<TeacherCourseAssignment>();
}
