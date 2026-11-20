namespace SchoolManagement.Domain;

public class Course
{
    public int CourseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<TeacherCourseAssignment> Assignments { get; set; } =
        new List<TeacherCourseAssignment>();
}
