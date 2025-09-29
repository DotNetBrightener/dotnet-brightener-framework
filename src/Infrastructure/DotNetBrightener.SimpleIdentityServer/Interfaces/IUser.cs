namespace DotNetBrightener.SimpleIdentityServer.Interfaces;

public interface IUser
{
    long Id { get; }

    string UserName { get; }
}

public interface IUserStore;