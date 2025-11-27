using DotNetBrightener.SiteSettings.Abstractions;
using DotNetBrightener.SiteSettings.Internal;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SiteSettingsController(
    ISiteSettingService                      siteSettingService,
    IStringLocalizer<SiteSettingsController> stringLocalizer)
    : Controller
{
    private readonly IStringLocalizer T = stringLocalizer;

    [HttpGet("allSettings")]
    public virtual IActionResult GetAllSettings()
    {
        var siteSettings = siteSettingService.GetAllAvailableSettings();

        return Ok(siteSettings);
    }

    [HttpGet("{settingKey}")]
    [AllowAnonymous]
    public virtual async Task<IActionResult> GetSetting(string settingKey)
    {
        if (!TryGetSettingInstance(settingKey, out var siteSettingInstance, out var returnAction))
            return returnAction;

        var siteSettingValue = await siteSettingService.GetSettingAsync(siteSettingInstance.GetType());

        return Ok(siteSettingValue);
    }

    [HttpGet("{settingKey}/default")]
    public virtual async Task<IActionResult> GetDefaultSetting(string settingKey)
    {
        if (!TryGetSettingInstance(settingKey, out var siteSettingInstance, out var returnAction))
            return returnAction;

        var siteSettingValue = await siteSettingService.GetSettingAsync(siteSettingInstance.GetType(), true);

        return Ok(siteSettingValue);
    }

    [HttpPost("{settingKey}")]
    [SettingJsonBodyReader]
    public virtual async Task<IActionResult> SaveSetting(string settingKey)
    {
        try
        {
            if (!TryGetSettingInstance(settingKey, 
                                       out var siteSettingInstance, 
                                       out var returnAction))
                return returnAction;

            var body = Request.HttpContext
                              .Items[SettingJsonBodyReader.RequestBodyKey]
                              .ToString()!;

            var settingType = siteSettingInstance.GetType();

            var formModel = JsonConvert.DeserializeObject(body, settingType) as SiteSettingBase;

            await siteSettingService.SaveSettingAsync(formModel, settingType);

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
        siteSettingInstance = siteSettingService.GetSettingInstance(settingKey);

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