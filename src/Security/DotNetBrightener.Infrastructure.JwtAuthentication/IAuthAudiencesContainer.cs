using Microsoft.Extensions.DependencyInjection;
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
    void RegisterValidAudiences(params string [ ] validAudiences);

    /// <summary>
    ///     Removes the valid audiences from the container
    /// </summary>
    /// <param name="validAudiences"></param>
    void RemoveAudiences(params string[] validAudiences);

    Task<bool> IsValidAudience(string audienceString);
}

public class DefaultAuthAudiencesContainer : IAuthAudiencesContainer
{
    private readonly        List<string>                    _validAudiences = new();
    private readonly        IServiceScopeFactory            _serviceScopeFactory;
    private                 bool                            _initialized = false;
    private static readonly object                          _lockObject  = new();
    private readonly        ILogger                         _logger;
    private                 TimeBaseCancellationTokenSource _refreshAudienceTimeout;

    public DefaultAuthAudiencesContainer(ILogger<DefaultAuthAudiencesContainer> logger,
                                         IServiceScopeFactory                   serviceScopeFactory)
    {
        _logger                   = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public IEnumerable<string> ValidAudiences
    {
        get
        {
            if (_refreshAudienceTimeout is null ||
                _refreshAudienceTimeout.IsCancellationRequested)
                _initialized = false;

            if (!_initialized)
            {
                lock (_lockObject)
                {
                    EnsureInitialized();
                }
            }

            _logger.LogInformation("Valid Audiences requested. Found {count} of valid audiences",
                                   _validAudiences.Count);

            return _validAudiences;
        }
    }

    public void RegisterValidAudiences(params string[] validAudiences)
    {
        foreach (string validAudience in validAudiences)
        {
            if (!_validAudiences.Contains(validAudience))
                _validAudiences.Add(validAudience);
        }
    }

    public void RemoveAudiences(params string[] validAudiences)
    {
        lock (_lockObject)
        {
            foreach (var validAudience in validAudiences)
            {
                _validAudiences.Remove(validAudience);
            }
        }
    }

    public void EnsureInitialized()
    {
        if (!_initialized)
        {
            lock (_lockObject)
            {
                _refreshAudienceTimeout?.Dispose();

                _refreshAudienceTimeout = new TimeBaseCancellationTokenSource(TimeSpan.FromMinutes(2));

                LoadAudiences();

                _initialized = true;
            }
        }
    }

    private void LoadAudiences()
    {
        _validAudiences.Clear();

        using var scope              = _serviceScopeFactory.CreateScope();
        var       serviceProvider    = scope.ServiceProvider;
        var       audienceValidators = serviceProvider.GetServices<IAuthAudienceValidator>();

        foreach (IAuthAudienceValidator validator in audienceValidators)
        {
            var validAudiences = validator.GetValidAudiences();

            if (validAudiences.Length > 0)
            {
                RegisterValidAudiences(validAudiences);
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