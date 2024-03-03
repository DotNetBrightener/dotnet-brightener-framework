using System.Net;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class LocaleAlreadyExistsError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public LocaleAlreadyExistsError(IStringLocalizer<LocaleAlreadyExistsError> localizer)
        : base(localizer)
    {

    }
}