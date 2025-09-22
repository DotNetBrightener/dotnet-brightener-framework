namespace WebApp.CommonShared.AsyncTasks;

public interface IAsyncTask<in TInput> where TInput : class
{
    Task<dynamic> Execute(TInput taskInput, AsyncTaskContext context);
}