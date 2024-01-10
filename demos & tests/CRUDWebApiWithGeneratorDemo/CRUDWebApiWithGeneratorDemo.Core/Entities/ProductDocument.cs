using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

public class ProductDocument : BaseEntityWithAuditInfo
{
    [MaxLength(255)]
    public string FileName { get; set; }

    [MaxLength(1024)]
    public string Description { get; set; }

    [MaxLength(2048)]
    public string FileUrl { get; set; }

    public Guid? FileGuid { get; set; }

    public int? DisplayOrder { get; set; }

    [DataType(DataType.Currency)]
    public decimal? Price { get; set; }
}