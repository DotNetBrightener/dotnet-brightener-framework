using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class TestEntity : BaseEntityWithAuditInfo
{
    public string Name { get; set; }

    public string Description { get; set; }
}