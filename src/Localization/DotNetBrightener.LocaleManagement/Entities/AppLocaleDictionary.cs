using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.Models;

namespace LocaleManagement.Entities;

/// <summary>
///     Represents the dictionary for a specific app and locale
/// </summary>
public class AppLocaleDictionary : BaseEntityWithAuditInfo
{
    /// <summary>
    ///     The Identifier of the associated app
    /// </summary>
    [MaxLength(155)]
    public string AppUniqueId { get; set; }

    /// <summary>
    ///     The locale (culture) code, usually is the language code and the country code, eg. en-US, es-US, vi-VN
    /// </summary>
    [MaxLength(16)]
    public string LocaleCode { get; set; }

    /// <summary>
    ///     Name of the associated app
    /// </summary>
    [MaxLength(255)]
    public string AppName { get; set; }

    /// <summary>
    ///     The language code
    /// </summary>
    [MaxLength(5)]
    public string LanguageCode { get; set; }

    /// <summary>
    ///     The country code
    /// </summary>
    [MaxLength(5)]
    public string CountryCode { get; set; }

    /// <summary>
    ///     The display name of the language
    /// </summary>
    [MaxLength(255)]
    public string DisplayName { get; set; }

    /// <summary>
    ///     The description of the app locale
    /// </summary>
    [MaxLength(1024)]
    public string Description { get; set; }

    /// <summary>
    ///     Indicates that the locale is the default for the given app
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    ///     Indicates that the locale is active / enabled for selecting in the associated app
    /// </summary>
    public bool IsActive { get; set; }
}