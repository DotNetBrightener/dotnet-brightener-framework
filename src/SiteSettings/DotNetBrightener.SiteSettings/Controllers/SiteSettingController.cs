using DotNetBrightener.SiteSettings.Abstractions;
using DotNetBrightener.SiteSettings.Internal;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotNetBrightener.SiteSettings.Controllers;

[ApiController]
public abstract class SiteSettingBaseController<TSettingType>(ISiteSettingService siteSettingService) : Controller
    where TSettingType : SiteSettingBase, new()
{
    protected virtual Task<bool> CanRetrieveSetting() => Task.FromResult(true);

    protected virtual Task<bool> CanSaveSetting() => Task.FromResult(true);

    [HttpGet]
    public virtual async Task<IActionResult> GetSetting()
    {
        if (!await CanRetrieveSetting())
            return Forbid();

        var siteSettingValue = await siteSettingService.GetSettingAsync<TSettingType>();

        return Ok(siteSettingValue);
    }

    [HttpGet("default")]
    public virtual async Task<IActionResult> GetDefaultSetting()
    {
        if (!await CanRetrieveSetting())
            return Forbid();

        var siteSettingValue = await siteSettingService.GetSettingAsync(typeof(TSettingType), true);

        return Ok(siteSettingValue);
    }

    [HttpPost]
    [SettingJsonBodyReader]
    public virtual async Task<IActionResult> SaveSetting()
    {
        if (!await CanSaveSetting())
            return Forbid();

        try
        {
            if (!TryGetSettingInstance(typeof(TSettingType).FullName,
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
            returnAction = NotFound();

            return false;
        }

        returnAction = null;

        return true;
    }
}