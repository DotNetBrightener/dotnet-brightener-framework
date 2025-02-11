﻿using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace WebApp.CommonShared.Mvc;

public class ETagFilterAttribute : Attribute, IActionFilter
{
    private readonly int[] _statusCodes;

    public ETagFilterAttribute(params int[] statusCodes)
    {
        _statusCodes = statusCodes;
        if (statusCodes.Length == 0) _statusCodes = new[] { 200 };
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Request.Method == "GET")
        {
            if (_statusCodes.Contains(context.HttpContext.Response.StatusCode))
            {
                var content = JsonConvert.SerializeObject(context.Result);

                var etag = ETagGenerator.GetETag(context.HttpContext.Request.Path.ToString(),
                                                 Encoding.UTF8.GetBytes(content));

                if (context.HttpContext.Request.Headers.Keys.Contains("If-None-Match") &&
                    context.HttpContext.Request.Headers["If-None-Match"].ToString() == etag)
                {
                    context.Result = new StatusCodeResult(304);
                }

                context.HttpContext.Response.Headers.Add("ETag",
                                                         new[]
                                                         {
                                                             etag
                                                         });
            }
        }
    }
}

public static class ETagGenerator
{
    public static string GetETag(string key, byte[] contentBytes)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var combinedBytes = Combine(keyBytes, contentBytes);

        return GenerateETag(combinedBytes);
    }

    private static string GenerateETag(byte[] data)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(data);
            string hex = BitConverter.ToString(hash);
            return hex.Replace("-", "");
        }
    }

    private static byte[] Combine(byte[] a, byte[] b)
    {
        byte[] c = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, c, 0, a.Length);
        Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
        return c;
    }
}
