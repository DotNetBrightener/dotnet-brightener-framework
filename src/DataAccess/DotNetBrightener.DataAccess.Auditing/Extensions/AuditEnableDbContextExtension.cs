#nullable enable
using DotNetBrightener.DataAccess.EF.Converters;
using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class AuditEnableDbContextExtension
{
    public static void EnableAuditing(this IServiceCollection serviceCollection)
    {

    }
}