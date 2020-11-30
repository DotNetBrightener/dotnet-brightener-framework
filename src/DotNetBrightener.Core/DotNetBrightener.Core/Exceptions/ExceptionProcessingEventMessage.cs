using System;
using System.Net;
using DotNetBrightener.Core.Events;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.Core.Exceptions
{
    public class ExceptionProcessingEventMessage: BaseEventMessage
    {
        public DefaultErrorResult ErrorResult { get; set; }

        public Exception ContextException { get; set; }

        public IActionResult ProcessResult { get; internal set; }

        public HttpStatusCode? StatusCode { get; set; }

        public void SetProcessResult(IActionResult result)
        {
            ProcessResult = result;
        }
    }
}