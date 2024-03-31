using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;

namespace WebApp.CommonShared.Exceptions;

public class NotFoundError : BaseProblemDetailsError
{
    public override string ErrorCode => "GEN-0404";

    public override string Summary   => "Represents the error when a request to a resource could not be found.";

    public override string DetailReason => "The requested resource could not be found";

    public NotFoundError() : this("Not Found")
    {

    }

    public NotFoundError(string message) : base(message, HttpStatusCode.NotFound)
    {

    }
}