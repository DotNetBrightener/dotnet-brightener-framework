using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public abstract class LocaleManagementBaseErrorResult
{
    public string ErrorMessage { get; set; }

    public string FullErrorMessage { get; set; }

    public long? ErrorId { get; set; }

    public string StackTrace { get; set; }

    public string TenantName { get; internal set; }

    public object Data { get; set; }

    public string ErrorType => GetType().Name;

    public virtual int StatusCode => 500;

    public string MessageLocalizationKey { get; set; }

    [JsonIgnore]
    protected IStringLocalizer? Localizer = null;

    protected LocaleManagementBaseErrorResult()
    {
        MessageLocalizationKey = $"dnb-locale-management.error-messages.{GetType().Name.TitleCaseToHyphenSeparated()}";
        FullErrorMessage       = MessageLocalizationKey;
    }

    protected LocaleManagementBaseErrorResult(IStringLocalizer? localizer)
        : this()
    {
        Localizer   = localizer;
        FullErrorMessage = Localizer?[MessageLocalizationKey] ?? MessageLocalizationKey;
    }
}