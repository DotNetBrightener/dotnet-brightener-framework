using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface IAuthAudiencesContainer
{
    IEnumerable<string> ValidAudiences
    {
        get;
    }

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

    public DefaultAuthAudiencesContainer(IHttpContextAccessor                httpContextAccessor,
                                         IEnumerable<IAuthAudienceValidator> audienceValidators)
    {
        _httpContextAccessor = httpContextAccessor;
        _audienceValidators  = audienceValidators;
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

    public void RegisterValidAudience(params string [ ] validAudiences)
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

    public Task<bool> IsValidAudience(string audienceString)
    {
        var audiences = audienceString.Split(new[]
                                             {
                                                 ";", ","
                                             },
                                             StringSplitOptions.RemoveEmptyEntries);

        return Task.FromResult(_validAudiences.Any(_ => audiences.Contains(_)));
    }
}