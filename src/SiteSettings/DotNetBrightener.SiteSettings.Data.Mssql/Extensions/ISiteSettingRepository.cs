﻿using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.SiteSettings.Data.Mssql.Data;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SiteSettings.Data.Mssql.Extensions;

internal interface ISiteSettingRepository : IRepository;

internal class SiteSettingRepository(
    MssqlStorageSiteSettingDbContext dbContext,
    IServiceProvider                 serviceProvider,
    ILoggerFactory                   loggerFactory)
    : Repository(dbContext, serviceProvider, loggerFactory), ISiteSettingRepository;