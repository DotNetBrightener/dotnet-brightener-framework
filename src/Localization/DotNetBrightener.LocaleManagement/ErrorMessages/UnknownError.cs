using System.Net;

namespace DotNetBrightener.LocaleManagement.ErrorMessages;

public class UnknownError : LocaleManagementBaseErrorResult
{
    public override int StatusCode => (int)HttpStatusCode.InternalServerError;

    public UnknownError()
    {

    }
}