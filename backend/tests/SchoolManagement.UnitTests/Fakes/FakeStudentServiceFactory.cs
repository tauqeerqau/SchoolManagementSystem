using SchoolManagement.Application.Students;

namespace SchoolManagement.UnitTests.Fakes;

internal static class FakeStudentServiceFactory
{
    public static StudentService Create(
        FakeStudentReadStore? readStore = null,
        FakeStudentCreateStore? createStore = null,
        FakeStudentUpdateStore? updateStore = null,
        FakeStudentDeleteStore? deleteStore = null) => new(
            new FakeStudentStore(
                readStore ?? new FakeStudentReadStore([]),
                createStore ?? new FakeStudentCreateStore(0),
                updateStore ?? new FakeStudentUpdateStore(studentFound: false),
                deleteStore ?? new FakeStudentDeleteStore(studentExists: false)));
}
