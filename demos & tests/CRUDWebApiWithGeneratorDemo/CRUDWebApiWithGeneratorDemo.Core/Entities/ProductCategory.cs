using DotNetBrightener.DataAccess.Models;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class ProductCategory : BaseEntityWithAuditInfo
{
    public string Name { get; set; }
}