using DotNetBrightener.Mapper.Mapping;
using MapperDashboardDemo.DtoTargets;
using MapperDashboardDemo.Entities;
using MapperDashboardDemo.Services;

namespace MapperDashboardDemo.Endpoints;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companies");

        group.MapGet("/", (SeedDataService data) =>
        {
            return data.Companies.SelectTargets<Company, CompanyDto>();
        });

        group.MapGet("/{id:int}", (int id, SeedDataService data) =>
        {
            var company = data.Companies.FirstOrDefault(c => c.Id == id);
            if (company is null)
                return Results.NotFound();

            return Results.Ok(company.ToTarget<Company, CompanyDto>());
        });
    }
}
