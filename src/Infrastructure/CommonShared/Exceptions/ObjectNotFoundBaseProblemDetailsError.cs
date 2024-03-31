using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;

namespace WebApp.CommonShared.Exceptions;

public abstract class ObjectNotFoundBaseProblemDetailsError<T> : BaseProblemDetailsError where T : class
{
    public override string Summary =>
        @"The error represents issue when a given object of type " + typeof(T).Name + @" could not be found.";

    public override string DetailReason => "The requested resource of type " + typeof(T).Name + " could not be found";

    public ObjectNotFoundBaseProblemDetailsError()
        : this("Object of type " + typeof(T).Name + " Not Found")
    {

    }

    public ObjectNotFoundBaseProblemDetailsError(string message)
        : base(message, HttpStatusCode.NotFound)
    {

    }
}