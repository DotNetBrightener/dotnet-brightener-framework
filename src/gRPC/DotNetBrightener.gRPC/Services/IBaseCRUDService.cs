using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.GenericCRUD.Extensions;
using DotNetBrightener.GenericCRUD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetBrightener.gRPC.Services;

public interface IBaseCRUDService<TEntityType> where TEntityType: class, new()
{
    Task<PagedCollection> GetList(BaseQueryModel request,
                                  Func<IQueryable<TEntityType>, IEnumerable<TEntityType>>
                                      postProcessing = null);

    Task<TEntityType> GetItem(long id, BaseQueryModel request = null);

    Task<TEntityType> CreateItem(TEntityType item);

    Task<TEntityType> UpdateItem(long id, TEntityType item);

    Task DeleteItem(long id);

    Task<TEntityType> RestoreItem(long id);
}

public abstract class BaseCrudService<TEntityType> : IBaseCRUDService<TEntityType> where TEntityType : class, new()
{
    protected readonly IBaseDataService<TEntityType> DataService;

    protected virtual string EntityIdColumnName => nameof(BaseEntity.Id);

    protected virtual string DefaultSortColumnName => nameof(BaseEntityWithAuditInfo.CreatedDate);

    protected BaseCrudService(IBaseDataService<TEntityType> dataService)
    {
        DataService = dataService;
    }

    public virtual async Task<PagedCollection> GetList(BaseQueryModel request,
                                                       Func<IQueryable<TEntityType>, IEnumerable<TEntityType>>
                                                           postProcessing = null)
    {
        var entitiesQuery = DataService.FetchActive();

        var filterDictionary = request.QueryDictionary;

        var columnsToPick = request.FilteredColumns;

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterDictionary);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var defaultSortColumnName = DefaultSortColumnName;

        if (!typeof(TEntityType).IsAssignableTo(typeof(BaseEntityWithAuditInfo)))
        {
            defaultSortColumnName = EntityIdColumnName;
        }

        var orderedQuery = entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                                       defaultSortColumnName,
                                                                       out var pageSize,
                                                                       out var pageIndex,
                                                                       postProcessing);

        var finalQuery = orderedQuery.PerformColumnsSelectorQuery(columnsToPick);

        var result = finalQuery.ToDynamicArray();

        return new PagedCollection
        {
            Items       = result,
            TotalCount  = totalRecords,
            PageIndex   = pageIndex,
            PageSize    = pageSize,
            ResultCount = result.Length
        };
    }

    public virtual async Task<TEntityType> GetItem(long id, BaseQueryModel request = null)
    {
        var expression = EntityIdColumnName.EqualsTo<TEntityType>(id);

        var entityItemQuery = request?.DeletedRecordsOnly == true
                                  ? DataService.FetchDeletedRecords()
                                  : DataService.FetchActive();

        entityItemQuery = entityItemQuery.Where(expression);

        var finalQuery = entityItemQuery.PerformColumnsSelectorQuery(request?.FilteredColumns);

        var item = (await finalQuery.ToDynamicArrayAsync()).FirstOrDefault();

        var itemInstance = Activator.CreateInstance<TEntityType>();

        ((object)item).CopyTo(itemInstance);

        return itemInstance;
    }

    public virtual Task<TEntityType> CreateItem(TEntityType item)
    {
        throw new System.NotImplementedException();
    }

    public virtual Task<TEntityType> UpdateItem(long id, TEntityType item)
    {
        throw new System.NotImplementedException();
    }

    public virtual Task DeleteItem(long id)
    {
        throw new System.NotImplementedException();
    }

    public virtual Task<TEntityType> RestoreItem(long id)
    {
        throw new System.NotImplementedException();
    }
}