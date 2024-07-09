using DotNetBrightener.DataAccess.Models;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class ProductCategory : BaseEntityWithAuditInfo
{
    public string Name { get; set; }

    public virtual ICollection<Product> Products { get; set; }
}

public class GroupEntity : BaseEntityWithAuditInfo
{
    public string Name { get; set; }
}