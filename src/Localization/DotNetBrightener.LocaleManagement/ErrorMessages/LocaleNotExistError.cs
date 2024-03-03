using System.Net;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class LocaleNotExistError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public LocaleNotExistError(IStringLocalizer<LocaleNotExistError> localizer)
        : base(localizer)
    {

    }
}