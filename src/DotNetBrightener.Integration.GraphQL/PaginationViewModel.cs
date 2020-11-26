using System.Linq;

namespace DotNetBrightener.Integration.GraphQL
{
    /// <summary>
    ///     Represents the paged configuration of a particular list of entities
    /// </summary>
    public class PaginationViewModel<TEntity, TEntityGraphType>
    {
        public int TotalRecords { get; set; }
        public int PageSize     { get; set; }
        public int PageIndex    { get; set; }

        public IQueryable<TEntity> Data { get; set; }
    }
}