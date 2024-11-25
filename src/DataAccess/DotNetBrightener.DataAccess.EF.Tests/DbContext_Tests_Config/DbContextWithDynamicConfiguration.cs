using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Tests.DbContext_Tests_Config;

public class DbContextWithDynamicConfiguration(DbContextOptions<DbContextWithDynamicConfiguration> options)
    : AdvancedDbContext(options);