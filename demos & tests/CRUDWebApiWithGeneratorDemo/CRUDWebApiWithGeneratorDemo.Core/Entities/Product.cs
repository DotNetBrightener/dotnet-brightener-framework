using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.DataAccess.Models;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class Product: BaseEntityWithAuditInfo
{
    public string Name { get; set; }

    public string Description { get; set; }

    public long? ProductCategoryId { get; set; }

    [ForeignKey(nameof(ProductCategoryId))]
    public virtual ProductCategory ProductCategory { get; set; }
}