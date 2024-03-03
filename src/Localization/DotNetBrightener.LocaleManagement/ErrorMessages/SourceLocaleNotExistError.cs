using System.Net;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class SourceLocaleNotExistError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public SourceLocaleNotExistError(IStringLocalizer<SourceLocaleNotExistError> localizer)
        : base(localizer)
    {

    }
}