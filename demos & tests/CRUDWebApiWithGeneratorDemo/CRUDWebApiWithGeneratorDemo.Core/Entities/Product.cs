using DotNetBrightener.DataAccess.Models;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class Product: BaseEntityWithAuditInfo
{
    public string Name { get; set; }

    public string Description { get; set; }
}