using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.LocaleManagement.Entities;

/// <summary>
///     Represents the dictionary entry for a specific locale
/// </summary>
[HistoryEnabled]
public class DictionaryEntry : BaseEntityWithAuditInfo
{
    public long DictionaryId { get; set; }

    [MaxLength(512)]
    public string Key { get; set; }

    [MaxLength(4000)]
    public string Value { get; set; }

    [MaxLength(1024)]
    public string Description { get; set; }

    [ForeignKey(nameof(DictionaryId))]
    public virtual AppLocaleDictionary AppLocaleDictionary { get; set; }
}