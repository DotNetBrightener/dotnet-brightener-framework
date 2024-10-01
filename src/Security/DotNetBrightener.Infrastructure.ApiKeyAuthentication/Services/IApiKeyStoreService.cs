using DotNetBrightener.Caching;
using DotNetBrightener.CryptoEngine;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Internal;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Models;
using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Permissions;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Services;

public interface IApiKeyStoreService
{
    Task<ApiKey> AuthorizeKey(string apiToken);

    Task<IEnumerable<ApiKey>> RetrieveAllApiKeys();

    Task<ApiKey> RetrieveApiKey(string tokenId);

    Task<string> GenerateAndStoreToken(string tokenName, string[] scopes, int? expirationInDays);

    Task<string> Regenerate(string tokenId);

    bool DeleteToken(string tokenId);
}

public abstract class BaseApiKeyStoreService : IApiKeyStoreService
{
    private readonly IPasswordValidationProvider _passwordValidationProvider;
    private readonly ICryptoEngine               _cryptoEngine;
    private readonly ICacheManager               _cacheManager;
    private const    string                      TokenJoinSeparator = "::";

    protected BaseApiKeyStoreService(IPasswordValidationProvider passwordValidationProvider,
                                     ICacheManager               cacheManager,
                                     ICryptoEngine               cryptoEngine)
    {
        _passwordValidationProvider = passwordValidationProvider;
        _cacheManager               = cacheManager;
        _cryptoEngine               = cryptoEngine;
    }

    public Task<ApiKey> AuthorizeKey(string apiToken)
    {
        var apiKey = _cacheManager.GetAsync<ApiKey>(new CacheKey($"api:token:{apiToken}", cacheTime: 10),
                                                    () => InternalAuthorizeKey(apiToken));

        return apiKey;
    }

    public abstract Task<IEnumerable<ApiKey>> RetrieveAllApiKeys();

    public Task<string> GenerateAndStoreToken(string tokenName, string[] scopes, int? expirationInDays)
    {
        var randomToken = GeneratePlainToken();

        var passwords = _passwordValidationProvider.GenerateEncryptedPassword(randomToken.TokenPassword);

        StoreApiToken(randomToken.TokenId,
                      tokenName,
                      passwords.Item1,
                      passwords.Item2,
                      scopes.Except(new[]
                             {
                                 ApiKeyAuthPermissions.ManageApiKeys
                             })
                            .ToArray(),
                      expirationInDays.HasValue ? DateTime.UtcNow.Date.AddDays(expirationInDays.Value) : null);

        return Task.FromResult(randomToken.EncryptedToken);
    }

    public async Task<string> Regenerate(string tokenId)
    {
        var storedToken = await RetrieveApiKey(tokenId);

        if (storedToken != null)
        {
            int? expirationInDays = null;

            if (storedToken.ExpiresAtUtc.HasValue)
            {
                expirationInDays = (int)(storedToken.ExpiresAtUtc.Value - DateTime.UtcNow.Date).TotalDays;
            }

            var newTokenString = await GenerateAndStoreToken(storedToken.Name, storedToken.Scopes, expirationInDays);

            DeleteToken(tokenId);

            return newTokenString;
        }

        return null;
    }

    public abstract bool DeleteToken(string tokenId);

    public abstract Task<ApiKey> RetrieveApiKey(string tokenId);

    protected abstract ApiKey StoreApiToken(string    tokenId,
                                            string    tokenName,
                                            string    tokenSalt,
                                            string    tokenHashed,
                                            string[]  scopes,
                                            DateTime? expiresOnUtc);

    protected virtual async Task<ApiKey> InternalAuthorizeKey(string apiToken)
    {
        var tokenSegment = DecryptToken(apiToken);

        var storedToken = await RetrieveApiKey(tokenId: tokenSegment.TokenId);

        if (storedToken != null &&
            _passwordValidationProvider.ValidatePassword(tokenSegment.TokenPassword,
                                                         storedToken.SaltValue,
                                                         storedToken.ApiKeyHashedToken))
        {
            return storedToken;
        }

        return null;
    }

    private TokenSegment GeneratePlainToken(string tokenId = null)
    {
        var tokenSegment = new TokenSegment
        {
            TokenId       = tokenId ?? Uuid7.Guid().ToString(),
            TokenPassword = CryptoUtilities.CreateRandomToken(32)
        };

        var tokenFormat = string.Join(TokenJoinSeparator,
                                      tokenSegment.TokenPassword,
                                      tokenSegment.TokenId);

        tokenSegment.EncryptedToken = _cryptoEngine.EncryptText(tokenFormat);

        return tokenSegment;
    }

    private TokenSegment DecryptToken(string plainToken)
    {
        var decryptedToken = _cryptoEngine.DecryptText(plainToken);
        var tokenSegments = decryptedToken.Split(new[]
                                                 {
                                                     TokenJoinSeparator
                                                 },
                                                 StringSplitOptions.None);

        if (tokenSegments.Length != 2)
        {
            return null;
        }

        return new TokenSegment(tokenSegments[0], tokenSegments[1]);
    }

    protected class TokenSegment
    {
        public TokenSegment()
        {

        }

        public TokenSegment(string tokenPassword, string tokenId)
        {
            TokenPassword = tokenPassword;
            TokenId       = tokenId;
        }

        public string TokenPassword { get; set; }

        public string TokenId { get; set; }

        public string EncryptedToken { get; set; }
    }
}