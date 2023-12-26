using System;
using System.Net;
using DotNetBrightener.WebApp.CommonShared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.WebApp.CommonShared.Exceptions;

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