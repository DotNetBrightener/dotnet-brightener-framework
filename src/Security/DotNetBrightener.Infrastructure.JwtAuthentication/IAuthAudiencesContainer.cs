using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface IAuthAudiencesContainer
{
    /// <summary>
    ///     The list of valid audiences to check against when JWT validation is performed
    /// </summary>
    IEnumerable<string> ValidAudiences
    {
        get;
    }

    /// <summary>
    ///     Registers the valid audiences to the container
    /// </summary>
    /// <param name="validAudiences"></param>
    void RegisterValidAudience(params string [ ] validAudiences);

    void EnsureInitialized();

    Task<bool> IsValidAudience(string audienceString);
}

public class DefaultAuthAudiencesContainer : IAuthAudiencesContainer
{
    private readonly        IHttpContextAccessor                _httpContextAccessor;
    private readonly        List<string>                        _validAudiences = new List<string>();
    private readonly        IEnumerable<IAuthAudienceValidator> _audienceValidators;
    private                 bool                                _initialized = false;
    private static readonly object                              _lockObject  = new object();
    private readonly        ILogger                             _logger;

    public DefaultAuthAudiencesContainer(IHttpContextAccessor                   httpContextAccessor,
                                         IEnumerable<IAuthAudienceValidator>    audienceValidators,
                                         ILogger<DefaultAuthAudiencesContainer> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _audienceValidators  = audienceValidators;
        _logger         = logger;
    }

    public IEnumerable<string> ValidAudiences
    {
        get
        {
            if (_initialized)
            {
                return _validAudiences;
            }

            lock (_lockObject)
            {
                EnsureInitialized();
            }

            return _validAudiences;
        }
    }

    public void RegisterValidAudience(params string[] validAudiences)
    {
        foreach (string validAudience in validAudiences)
        {
            if (!_validAudiences.Contains(validAudience))
                _validAudiences.Add(validAudience);
        }
    }

    public void EnsureInitialized()
    {
        if (!_initialized)
        {
            lock (_lockObject)
            {
                foreach (IAuthAudienceValidator validator in _audienceValidators)
                {
                    validator.RegisterAudienceValidator(this);
                }

                _initialized = true;
            }
        }
    }

    public async Task<bool> IsValidAudience(string audienceString)
    {
        var audiences = audienceString.Split(new[]
                                             {
                                                 ";", ","
                                             },
                                             StringSplitOptions.RemoveEmptyEntries);


        var isValidAudience = _validAudiences.Any(validAudience => audiences.Contains(validAudience));

        _logger.LogInformation("Provided audience is {audienceValidity}.",
                               isValidAudience ? "valid" : "not valid");

        return isValidAudience;
    }
}