using Microsoft.AspNetCore.Mvc;
using System.Net;
using AspNet.Extensions.SelfDocumentedProblemResult;
using AspNet.Extensions.SelfDocumentedProblemResult.Exceptions;

namespace WebAppCommonShared.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExceptionTestController : ControllerBase
{
    [HttpGet("exception2/{userId}")]
    public IActionResult GetException2(long userId)
    {
        IProblemResult error = new UserNotFoundError(userId);

        return error.ToProblemResult();
    }
}

/// <summary>
///     The error represents the requested object of type `User` could not be found
/// </summary>
/// <remarks>
///     The error is thrown because the requested resource of type `User` could not be found
/// </remarks>
public class UserNotFoundError : BaseProblemDetailsError
{
    public UserNotFoundError()
        : base("Requested User Could Not Be Found", HttpStatusCode.NotFound)
    {

    }

    public UserNotFoundError(long userId)
        : this()
    {
        Data.Add("userId", userId);
    }

    public override string ErrorCode => "USR-0404";
}