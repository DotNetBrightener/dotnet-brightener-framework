using Microsoft.AspNetCore.Identity;

namespace DotNetBrightener.IdentityAuth.Models;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id            = Guid.CreateVersion7();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public User(string userName) : this()
    {
        UserName = userName;
    }
}