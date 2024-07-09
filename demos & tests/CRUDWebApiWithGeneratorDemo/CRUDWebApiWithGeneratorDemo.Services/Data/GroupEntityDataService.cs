using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

public partial class GroupEntityDataService
{
    private readonly ILogger _logger;

    public GroupEntityDataService(
            IRepository repository, 
            ILogger<GroupEntityDataService> logger)
        : this(repository)
    {
        _logger = logger;
    }

    // Implement your custom methods here
}