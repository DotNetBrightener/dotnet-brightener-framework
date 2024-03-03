using System.Net;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class DictionaryMustBeSpecifiedError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public DictionaryMustBeSpecifiedError(IStringLocalizer<DictionaryMustBeSpecifiedError> localizer)
        : base(localizer)
    {

    }
}

public class DictionaryEntryAlreadyExistsError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public DictionaryEntryAlreadyExistsError(IStringLocalizer<DictionaryEntryAlreadyExistsError> localizer)
        : base(localizer)
    {

    }
}