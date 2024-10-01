using DotNetBrightener.IdentityAuth.Internal;
using Microsoft.AspNetCore.Identity;

namespace DotNetBrightener.IdentityAuth.Models;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Uuid7.Guid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public User(string userName) : this()
    {
        UserName = userName;
    }
}