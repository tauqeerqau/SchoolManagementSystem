namespace SchoolManagement.Application.Students;

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<StudentDto?> GetByIdAsync(int studentId, CancellationToken cancellationToken);

    Task<StudentDto> CreateAsync(StudentCreateRequest request, CancellationToken cancellationToken);

    Task<StudentDto?> UpdateAsync(
        int studentId,
        StudentUpdateRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int studentId, CancellationToken cancellationToken);
}
