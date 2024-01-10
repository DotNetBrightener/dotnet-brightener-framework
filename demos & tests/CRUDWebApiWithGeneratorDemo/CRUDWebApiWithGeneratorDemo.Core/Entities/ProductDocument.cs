using DotNetBrightener.DataAccess.Models;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class ProductDocument: BaseEntityWithAuditInfo
{
    public string FileName { get; set; }

    public string Description { get; set; }

    public string FileUrl { get; set; }
}