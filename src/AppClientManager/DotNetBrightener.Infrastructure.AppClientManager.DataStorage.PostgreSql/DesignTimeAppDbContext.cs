﻿using Microsoft.EntityFrameworkCore.Design;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage.PostgreSql;

internal class DesignTimeAppDbContext : PostgreSqlDbContextDesignTimeFactory<MigrationPostgreSqlDbContext>
{
}