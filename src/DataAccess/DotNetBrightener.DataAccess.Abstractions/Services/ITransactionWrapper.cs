namespace DotNetBrightener.DataAccess.Services;

public interface ITransactionWrapper
{
    /// <summary>
    ///     Executes the given action within a transaction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    Task<T> ExecuteWithTransaction<T>(Func<T> action) where T : struct;

    /// <summary>
    ///     Executes the given action within a transaction
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task ExecuteWithTransaction(Action action);
}