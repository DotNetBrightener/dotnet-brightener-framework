using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Demo.WebApi.DbContexts;

namespace MultiTenancy.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TenantManagementController : ControllerBase
{
    
}


[ApiController]
[Route("api/[controller]")]
public class AllUsersController(IRepository repository) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var records = await repository.Fetch<User>()
                                      .ToListAsync();

        return Ok(records);
    }

    [HttpGet("clinics")]
    public async Task<IActionResult> GetClinics()
    {
        var records = await repository.Fetch<Clinic>()
                                      .ToListAsync();

        return Ok(records);
    }

    [HttpGet("tenantMappings")]
    public async Task<IActionResult> GetTenantMappings()
    {
        var records = await repository.Fetch<TenantEntityMapping>()
                                      .ToListAsync();

        return Ok(records);
    }
}