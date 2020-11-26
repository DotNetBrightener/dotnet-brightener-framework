using GraphQL.Types;

namespace DotNetBrightener.Integration.GraphQL
{
    public class PaginationObjectGraphType<TEntity> : ObjectGraphType where TEntity : IGraphType
    {
        public PaginationObjectGraphType()
        {
            Name = $"PaginationModelFor{typeof(TEntity).FullName.Replace(".", "_")}";

            Field<IntGraphType>("totalRecords");
            Field<ListGraphType<TEntity>>("data");
            Field<IntGraphType>("pageSize");
            Field<IntGraphType>("pageIndex");
        }
    }
}