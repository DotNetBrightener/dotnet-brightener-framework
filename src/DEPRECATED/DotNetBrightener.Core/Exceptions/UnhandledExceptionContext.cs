using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.Core.Exceptions;

public class UnhandledExceptionContext
{
    public DefaultErrorResult ErrorResult { get; set; }

    public Exception ContextException { get; set; }

    public IActionResult ProcessResult { get; set; }

    public HttpStatusCode? StatusCode { get; set; }

    public void SetProcessResult(IActionResult result)
    {
        ProcessResult = result;
    }
}