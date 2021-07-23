using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.DataAccess.EF.Repositories
{
    /// <summary>
    ///     The abstraction, empty implementation of the DbContext when using EntityFramework as data access layer.
    ///     The derived class should be used in order to register and define the entities
    /// </summary>
    public abstract class DotNetBrightenerDbContext : DbContext
    {
        protected DotNetBrightenerDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}