using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DotNetBrightener.MultiTenancy.DbContexts;

internal interface IInterceptorsEntriesContainer
{
    List<EntityEntry> InsertedEntityEntries { get; }
    List<EntityEntry> ModifiedEntityEntries { get; }
}

internal class InterceptorEntriesContainer : IInterceptorsEntriesContainer
{
    public List<EntityEntry> InsertedEntityEntries { get; } = [];
    public List<EntityEntry> ModifiedEntityEntries { get; } = [];
}