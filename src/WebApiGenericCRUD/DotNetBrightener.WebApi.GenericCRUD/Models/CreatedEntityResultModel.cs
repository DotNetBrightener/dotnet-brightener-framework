using DotNetBrightener.DataAccess.Models;
using Newtonsoft.Json;

namespace DotNetBrightener.GenericCRUD.Models;

public class CreatedEntityResultModel
{
    public long EntityId { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedDate { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? CreatedBy { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? ModifiedDate { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? ModifiedBy { get; set; }

    public CreatedEntityResultModel()
    {

    }

    public CreatedEntityResultModel(BaseEntity entity)
    {
        EntityId = entity.Id;
    }

    public CreatedEntityResultModel(BaseEntityWithAuditInfo auditableEntity)
    {
        EntityId     = auditableEntity.Id;
        CreatedDate  = auditableEntity.CreatedDate;
        CreatedBy    = auditableEntity.CreatedBy;
        ModifiedDate = auditableEntity.ModifiedDate;
        ModifiedBy   = auditableEntity.ModifiedBy;
    }
}