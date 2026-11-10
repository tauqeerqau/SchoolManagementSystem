using SchoolManagement.Application.Students;
using SchoolManagement.Domain;

namespace SchoolManagement.UnitTests.Fakes;

internal sealed class FakeStudentStore(
    FakeStudentReadStore readStore,
    FakeStudentCreateStore createStore,
    FakeStudentUpdateStore updateStore,
    FakeStudentDeleteStore deleteStore) : IStudentStore
{
    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken) =>
        readStore.GetAllAsync(cancellationToken);

    public Task<Student?> GetByIdAsync(int studentId, CancellationToken cancellationToken) =>
        readStore.GetByIdAsync(studentId, cancellationToken);

    public Task<Student> CreateAsync(Student student, CancellationToken cancellationToken) =>
        createStore.CreateAsync(student, cancellationToken);

    public Task<Student?> UpdateAsync(
        int studentId,
        Student updatedValues,
        CancellationToken cancellationToken) =>
        updateStore.UpdateAsync(studentId, updatedValues, cancellationToken);

    public Task<bool> DeleteAsync(int studentId, CancellationToken cancellationToken) =>
        deleteStore.DeleteAsync(studentId, cancellationToken);
}
