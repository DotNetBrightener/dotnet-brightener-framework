using DotNetBrightener.Core.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.DataAccess.EF.Repositories
{
    /// <summary>
    ///     The abstraction, empty implementation of the DbContext when using EntityFramework as data access layer.
    ///     The derived class should be used in order to register and define the entities
    /// </summary>
    public abstract class DotNetBrightenerDbContext : DbContext
    {
        protected IDataWorkContext DataWorkContext;

        /// <summary>
        ///     Retrieves the current logged in user's name for audit purpose
        /// </summary>
        protected string CurrentLoggedInUser => DataWorkContext.GetContextData<string>("CurrentUserName");

        /// <summary>
        ///     Retrieves the current logged in user's id for audit purpose
        /// </summary>
        protected long? CurrentLoggedInUserId => DataWorkContext.GetContextData<long?>("CurrentUserId");


        protected DotNetBrightenerDbContext(DbContextOptions options,
                                            IDataWorkContext dataWorkContext) : base(options)
        {
            DataWorkContext = dataWorkContext;
        }
    }
}