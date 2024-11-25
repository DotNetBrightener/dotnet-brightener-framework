using DotNetBrightener.DataAccess;
using Microsoft.EntityFrameworkCore;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public class EfDataServiceConfigurator
{
    internal IServiceCollection              ServiceCollection            { get; set; }
    internal Action<DbContextOptionsBuilder> SharedDbContextOptionBuilder { get; set; }
    internal DatabaseConfiguration           DbConfiguration              { get; set; }
}