namespace SchoolManagement.Domain;

public class TeacherCourseAssignment
{
    public int TeacherCourseAssignmentId { get; set; }

    public int TeacherId { get; set; }

    public int CourseId { get; set; }

    public int AcademicYear { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
