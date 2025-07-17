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

public class DefaultAuthAudiencesContainer(
    ILogger<DefaultAuthAudiencesContainer> logger,
    IServiceScopeFactory                   serviceScopeFactory)
    : IAuthAudiencesContainer
{
    private readonly        List<string>                    _validAudiences = new();
    private                 bool                            _initialized    = false;
    private static readonly Lock                            LockObject      = new();
    private readonly        ILogger                         _logger         = logger;
    private                 TimeBaseCancellationTokenSource _refreshAudienceTimeout;

    public IEnumerable<string> ValidAudiences
    {
        get
        {
            if (_refreshAudienceTimeout is null ||
                _refreshAudienceTimeout.IsCancellationRequested)
            {
                _logger.LogInformation("Refreshing list of audiences...");
                _initialized = false;
            }

            if (!_initialized)
            {
                lock (LockObject)
                {
                    EnsureInitialized();
                }
            }

            _logger.LogInformation("Valid audiences: {@validAudiences}", _validAudiences);

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
        lock (LockObject)
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
            lock (LockObject)
            {
                _refreshAudienceTimeout?.Dispose();
                _refreshAudienceTimeout = null;

                LoadAudiences();

                _refreshAudienceTimeout = new TimeBaseCancellationTokenSource(TimeSpan.FromMinutes(2));
                _initialized            = true;
            }
        }
    }

    private void LoadAudiences()
    {
        _validAudiences.Clear();

        using var scope              = serviceScopeFactory.CreateScope();
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
        var audiences = audienceString.Split([
                                                 ";", ","
                                             ],
                                             StringSplitOptions.RemoveEmptyEntries);


        var isValidAudience = _validAudiences.Any(validAudience => audiences.Contains(validAudience));

        _logger.LogInformation("Provided audience is {audienceValidity}.",
                               isValidAudience ? "valid" : "not valid");

        return isValidAudience;
    }
}