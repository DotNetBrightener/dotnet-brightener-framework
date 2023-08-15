using System;
using Newtonsoft.Json;

namespace DotNetBrightener.WebApi.GenericCRUD.Models;

public class CreatedEntityResultModel
{
    public long EntityId { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedDate { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string CreatedBy { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? ModifiedDate { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ModifiedBy { get; set; }
}