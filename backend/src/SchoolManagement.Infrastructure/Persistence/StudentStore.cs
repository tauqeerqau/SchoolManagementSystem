using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Students;
using SchoolManagement.Domain;

namespace SchoolManagement.Infrastructure.Persistence;

public sealed class StudentStore(SchoolManagementDbContext dbContext) : IStudentStore
{
    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Students
            .AsNoTracking()
            .OrderBy(student => student.StudentId)
            .ToListAsync(cancellationToken);

    public Task<Student?> GetByIdAsync(int studentId, CancellationToken cancellationToken) =>
        dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(student => student.StudentId == studentId, cancellationToken);

    public async Task<Student> CreateAsync(Student student, CancellationToken cancellationToken)
    {
        dbContext.Students.Add(student);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return student;
        }
        catch (DbUpdateException exception) when (IsStudentEmailUniqueViolation(exception))
        {
            throw new DuplicateStudentEmailException();
        }
    }

    public async Task<Student?> UpdateAsync(
        int studentId,
        Student updatedValues,
        CancellationToken cancellationToken)
    {
        var existingStudent = await dbContext.Students.FirstOrDefaultAsync(
            student => student.StudentId == studentId,
            cancellationToken);

        if (existingStudent is null)
        {
            return null;
        }

        existingStudent.FirstName = updatedValues.FirstName;
        existingStudent.LastName = updatedValues.LastName;
        existingStudent.Email = updatedValues.Email;
        existingStudent.DateOfBirth = updatedValues.DateOfBirth;
        existingStudent.EnrollmentDate = updatedValues.EnrollmentDate;
        existingStudent.IsActive = updatedValues.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return existingStudent;
        }
        catch (DbUpdateException exception) when (IsStudentEmailUniqueViolation(exception))
        {
            throw new DuplicateStudentEmailException();
        }
    }

    public async Task<bool> DeleteAsync(int studentId, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.FirstOrDefaultAsync(
            item => item.StudentId == studentId,
            cancellationToken);

        if (student is null)
        {
            return false;
        }

        dbContext.Students.Remove(student);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsStudentEmailUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException
        && sqlException.Number is 2601 or 2627
        && sqlException.Message.Contains("IX_Students_Email", StringComparison.Ordinal);
}
