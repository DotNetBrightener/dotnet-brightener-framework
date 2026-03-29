using MapperDashboardDemo.Entities;
using MapperDashboardDemo.Services;

namespace MapperDashboardDemo.Endpoints;

public static class BlogPostEndpoints
{
    public static void MapBlogPostEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/blog-posts");

        group.MapGet("/", (SeedDataService data) =>
        {
            return data.BlogPosts;
        });

        group.MapGet("/{id:int}", (int id, SeedDataService data) =>
        {
            var post = data.BlogPosts.FirstOrDefault(b => b.Id == id);
            if (post is null)
                return Results.NotFound();

            return Results.Ok(post);
        });
    }
}
