using System.Net;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class DictionaryEntryNotSupportedError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.NotFound;

    public DictionaryEntryNotSupportedError(IStringLocalizer<DictionaryEntryNotSupportedError> localizer)
        : base(localizer)
    {

    }
}