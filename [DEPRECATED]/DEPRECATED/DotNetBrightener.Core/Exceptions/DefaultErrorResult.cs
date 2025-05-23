﻿namespace DotNetBrightener.Core.Exceptions;

public class DefaultErrorResult
{
    public string ErrorMessage { get; set; }

    public string FullErrorMessage { get; set; }

    public long? ErrorId { get; set; }

    public string StackTrace { get; set; }

    public string TenantName { get; internal set; }

    public string ErrorType { get; set; }
}