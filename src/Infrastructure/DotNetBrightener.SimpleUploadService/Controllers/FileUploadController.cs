using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.SimpleUploadService.Models;
using DotNetBrightener.SimpleUploadService.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.Controllers;

public abstract class FileUploadController : ControllerBase
{
    protected readonly ILogger        Logger;
    protected readonly IUploadService UploadService;

    protected FileUploadController(IUploadService uploadService,
                                   ILogger        logger)
    {
        Logger        = logger;
        UploadService = uploadService;
    }

    protected async Task<List<FileObjectModel>> ProcessUploadFile(UploadRequestModel model)
    {
        var result = new List<FileObjectModel>();

        var files = Request.Form.Files.ToArray();

        // Loop through each file in the request
        foreach (var file in files)
        {
            try
            {
                var fileUploadResult = await UploadService.Upload(file.OpenReadStream(),
                                                                  model,
                                                                  file.FileName,
                                                                  Request.GetDisplayUrl());

                result.Add(fileUploadResult);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while uploading file");
                throw;
            }
        }

        return result;
    }
}