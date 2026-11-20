namespace SchoolManagement.Domain;

public class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public int AcademicYear { get; set; }

    public DateTime EnrolledOn { get; set; }

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
