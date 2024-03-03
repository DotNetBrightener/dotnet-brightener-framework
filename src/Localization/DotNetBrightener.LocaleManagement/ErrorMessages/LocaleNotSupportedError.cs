using System.Net;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class LocaleNotSupportedError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public LocaleNotSupportedError(IStringLocalizer<LocaleNotSupportedError> localizer)
        : base(localizer)
    {

    }
}