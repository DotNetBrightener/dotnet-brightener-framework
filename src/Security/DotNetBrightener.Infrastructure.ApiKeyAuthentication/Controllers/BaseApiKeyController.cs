using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Models;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Permissions;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;
using DotNetBrightener.Infrastructure.Security.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Controllers;

[ApiController]
[PermissionAuthorize(ApiKeyAuthPermissions.ManageApiKeys)]
public abstract class BaseApiKeyController: Controller
{
    private readonly IApiKeyStoreService _apiKeyStoreService;

    protected BaseApiKeyController(IApiKeyStoreService apiKeyStoreService)
    {
        _apiKeyStoreService = apiKeyStoreService;
    }

    [HttpGet("")]
    public virtual async Task<IActionResult> GetMyApiKeys()
    {
        var result = await _apiKeyStoreService.RetrieveAllApiKeys();
        
        return Ok(result);
    }

    [HttpGet("{tokenId}")]
    public virtual async Task<IActionResult> GetToken(string tokenId)
    {
        var result = await _apiKeyStoreService.RetrieveApiKey(tokenId);
        
        return Ok(result);
    }

    [HttpPost("")]
    public virtual async Task<IActionResult> GenerateApiKey([FromBody] GenerateApiKeyRequest requestModel)
    {
        var generatedToken = await _apiKeyStoreService.GenerateAndStoreToken(requestModel.Name,
                                                                             requestModel.Scopes,
                                                                             requestModel.ExpiresInDays);

        return Ok(new
        {
            token = generatedToken,
        });
    }

    [HttpPut("{tokenId}")]
    public virtual async Task<IActionResult> RegenerateToken(string tokenId)
    {
        var generatedToken = await _apiKeyStoreService.Regenerate(tokenId);

        return Ok(new
        {
            token = generatedToken,
        });
    }

    [HttpDelete("{tokenId}")]
    public virtual async Task<IActionResult> DeleteToken(string tokenId)
    {
        var generatedToken = _apiKeyStoreService.DeleteToken(tokenId);

        return Ok(new
        {
            token = generatedToken,
        });
    }
}