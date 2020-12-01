using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.Integration.GraphQL
{
    /// <summary>
    ///     Represents the paged configuration of a particular list of entities
    /// </summary>
    public class PaginationViewModel<TEntity, TEntityGraphType>
    {
        /// <summary>
        ///     The total number of the available records based on the given query
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        ///     The size of paged data
        /// </summary>
        public int PageSize     { get; set; }

        /// <summary>
        ///     The current index of the paged data
        /// </summary>
        public int PageIndex    { get; set; }

        /// <summary>
        ///     The collection of records
        /// </summary>
        public IEnumerable<TEntity> Data { get; set; }

        /// <summary>
        ///     Executes the query and returns the data for further joining operations if needed
        /// </summary>
        public void Execute()
        {
            Data = Data.ToArray();
        }
    }
}