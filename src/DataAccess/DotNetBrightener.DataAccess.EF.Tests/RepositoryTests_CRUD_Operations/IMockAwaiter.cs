namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public interface IMockAwaiter
{
    void WaitFinished(object expectedData);
}