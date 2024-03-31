using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;
using System.Net;

namespace LocaleManagement.ErrorMessages;

public class SourceLocaleNotExistError : BaseProblemDetailsError
{
    public override string DetailReason => "The error thrown when the source locale does not exist";

    public override string ErrorCode => "LM-0007";
    public override string Summary   => "If the source locale does not exist, it cannot be used to create a new locale";


    public static readonly SourceLocaleNotExistError Instance = new();

    public SourceLocaleNotExistError()
        : base("Source locale does not exist", HttpStatusCode.NotFound)
    {
    }
}