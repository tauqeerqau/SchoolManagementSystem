using SchoolManagement.Domain;

namespace SchoolManagement.Application.Students;

public interface IStudentStore
{
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken);

    Task<Student?> GetByIdAsync(int studentId, CancellationToken cancellationToken);

    Task<Student> CreateAsync(Student student, CancellationToken cancellationToken);

    Task<Student?> UpdateAsync(
        int studentId,
        Student updatedValues,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int studentId, CancellationToken cancellationToken);
}
