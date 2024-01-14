using DotNetBrightener.WebApi.GenericCRUD.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CRUDApiDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class TestEntityController : ControllerBase
{
    private readonly ILogger<TestEntityController> _logger;

    public TestEntityController(ILogger<TestEntityController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<TestEntity>> Get()
    {
        var deepPropertiesSearchFilters = Request.Query.ToDictionary(_ => _.Key,
                                                                     _ => _.Value.ToString());

        var entitiesQuery = await TestData.TestEntities
                                                  .AsQueryable()
                                                  .ApplyDeepFilters(deepPropertiesSearchFilters);

        return entitiesQuery.ToArray();
    }
}