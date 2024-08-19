using Microsoft.AspNetCore.Identity;

namespace DotNetBrightener.IdentityAuth.Models;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Ulid.NewUlid().ToGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public User(string userName) : this()
    {
        UserName = userName;
    }
}