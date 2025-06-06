﻿using DotNetBrightener.SiteSettings.Abstractions;
using DotNetBrightener.SiteSettings.Internal;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SiteSettingsController : Controller
{
    private readonly ISiteSettingService _siteSettingService;
    private readonly IStringLocalizer    T;

    public SiteSettingsController(ISiteSettingService                      siteSettingService,
                                  IStringLocalizer<SiteSettingsController> stringLocalizer)
    {
        _siteSettingService = siteSettingService;
        T                   = stringLocalizer;
    }

    [HttpGet("allSettings")]
    public IActionResult GetAllSettings()
    {
        var siteSettings = _siteSettingService.GetAllAvailableSettings();

        return Ok(siteSettings);
    }

    [HttpGet("{settingKey}")]
    [AllowAnonymous]
    public virtual IActionResult GetSetting(string settingKey)
    {
        if (!TryGetSettingInstance(settingKey, out var siteSettingInstance, out IActionResult returnAction))
            return returnAction;

        var siteSettingValue = _siteSettingService.GetSetting(siteSettingInstance.GetType());

        return Ok(siteSettingValue);
    }

    [HttpGet("{settingKey}/default")]
    public virtual IActionResult GetDefaultSetting(string settingKey)
    {
        if (!TryGetSettingInstance(settingKey, out var siteSettingInstance, out IActionResult returnAction))
            return returnAction;

        var siteSettingValue = _siteSettingService.GetSetting(siteSettingInstance.GetType(), true);

        return Ok(siteSettingValue);
    }

    [HttpPost("{settingKey}")]
    [SettingJsonBodyReader]
    public virtual IActionResult SaveSetting(string settingKey)
    {
        try
        {
            if (!TryGetSettingInstance(settingKey, out var siteSettingInstance, out IActionResult returnAction))
                return returnAction;

            var body = Request.HttpContext.Items[SettingJsonBodyReader.RequestBodyKey].ToString()!;

            var formModel = JsonConvert.DeserializeObject(body, siteSettingInstance.GetType()) as SiteSettingBase;

            _siteSettingService.SaveSetting(formModel, siteSettingInstance.GetType());

            return Ok();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception);
        }
    }

    private bool TryGetSettingInstance(string              settingKey,
                                       out SiteSettingBase siteSettingInstance,
                                       out IActionResult   returnAction)
    {
        siteSettingInstance = _siteSettingService.GetSettingInstance(settingKey);

        if (siteSettingInstance == null)
        {
            returnAction = NotFound(new
            {
                ErrorMessage = T["SiteSettings.MsgError.SettingNotFound", settingKey]
            });

            return false;
        }

        returnAction = null;

        return true;
    }
}