using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;
using Newtonsoft.Json;

namespace CRUDWebApiWithGeneratorDemo.Core.Entities;

/// <summary>
///     Represents the document of a product
/// </summary>
public class ProductDocument : BaseEntityWithAuditInfo
{
    /// <summary>
    ///     File name of the document
    /// </summary>
    [MaxLength(255)]
    public string FileName { get; set; }

    /// <summary>
    ///    Description of the document  
    /// </summary>
    [MaxLength(1024)]
    public string Description { get; set; }


    /// <summary>
    ///     File URL of the document
    /// </summary>
    [MaxLength(2048)]
    public string FileUrl { get; set; }

    /// <summary>
    ///     Unique identifier of the file
    /// </summary>
    public Guid? FileGuid { get; set; }

    /// <summary>
    ///    The display order of the document if it is displayed in a list
    /// </summary>
    public int? DisplayOrder { get; set; }

    /// <summary>
    ///     The price of the document
    /// </summary>
    [DataType(DataType.Currency)]
    [JsonIgnore]
    public decimal? Price { get; set; }
}