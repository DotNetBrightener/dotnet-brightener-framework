
using DotNetBrightener.IdentityAuth.Internal;
using Microsoft.AspNetCore.Identity;

namespace DotNetBrightener.IdentityAuth.Models;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Uuid7.Guid();
    }

    public Role(string roleName)
        : this()
    {
        Name = roleName;
    }
}