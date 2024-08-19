using Microsoft.AspNetCore.Identity;

namespace DotNetBrightener.IdentityAuth.Models;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Ulid.NewUlid().ToGuid();
    }

    public Role(string roleName) : this()
    {
        Name = roleName;
    }
}