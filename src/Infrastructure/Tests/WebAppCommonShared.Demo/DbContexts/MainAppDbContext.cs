using Microsoft.EntityFrameworkCore;

namespace WebAppCommonShared.Demo.DbContexts;

public class MainAppDbContext : DbContext
{
    public MainAppDbContext(DbContextOptions<MainAppDbContext> options)
        : base(options)
    {
    }
}