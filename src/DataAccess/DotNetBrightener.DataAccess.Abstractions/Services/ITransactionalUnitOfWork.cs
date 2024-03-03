using System;

namespace DotNetBrightener.DataAccess.Services;

/// <summary>
///     Represents a transactional unit-of-work, to support database rolling back
/// </summary>
public interface ITransactionalUnitOfWork : IDisposable
{
    /// <summary>
    ///     Marks the transaction should be rolled back due to some exceptions or intentionally
    /// </summary>
    void Rollback();
}