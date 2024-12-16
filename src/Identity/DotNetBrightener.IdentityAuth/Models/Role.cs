using Microsoft.AspNetCore.Identity;

namespace DotNetBrightener.IdentityAuth.Models;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Guid.CreateVersion7();
    }

    public Role(string roleName)
        : this()
    {
        Name = roleName;
    }
}