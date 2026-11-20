namespace SchoolManagement.Domain;

public class SchoolClass
{
    public int ClassId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
