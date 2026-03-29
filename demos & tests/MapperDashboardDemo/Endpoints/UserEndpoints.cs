using DotNetBrightener.Mapper.Mapping;
using MapperDashboardDemo.DtoTargets;
using MapperDashboardDemo.Entities;
using MapperDashboardDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace MapperDashboardDemo.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/", (SeedDataService data) =>
        {
            return data.Users.SelectTargets<User, UserListItemDto>();
        });

        group.MapGet("/{id:int}", (int id, SeedDataService data) =>
        {
            var user = data.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return Results.NotFound();

            return Results.Ok(user.ToTarget<User, UserDetailDto>());
        });

        group.MapGet("/{id:int}/summary", (int id, SeedDataService data) =>
        {
            var user = data.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return Results.NotFound();

            return Results.Ok(user.ToTarget<User, UserSummaryDto>());
        });

        group.MapGet("/{id:int}/edit", (int id, SeedDataService data) =>
        {
            var user = data.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return Results.NotFound();

            return Results.Ok(user.ToTarget<User, UserEditDto>());
        });

        group.MapPost("/from-dto", (UserEditDto dto) =>
        {
            var user = dto.ToSource<UserEditDto, User>();
            return Results.Ok(user);
        });

        group.MapGet("/{id:int}/full-name", (int id, SeedDataService data) =>
        {
            var user = data.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return Results.NotFound();

            return Results.Ok(user.ToTarget<User, UserWithFullNameDto>());
        });
    }
}
