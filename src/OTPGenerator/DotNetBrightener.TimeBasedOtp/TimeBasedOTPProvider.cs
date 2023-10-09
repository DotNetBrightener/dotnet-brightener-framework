using System.Runtime.Caching;

namespace DotNetBrightener.TimeBasedOtp;

internal class TimeBasedOTPProvider: IOTPProvider
{
    private static readonly MemoryCache       Cache = new(nameof(TimeBasedOTPProvider));
    private readonly        IDateTimeProvider _dateTimeProvider;

    /// <summary>
    ///     Leave option to disable cache so that the Unit Tests can work
    /// </summary>
    private readonly bool _disableCacheOtp = false;
    
    public TimeBasedOTPProvider(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    internal TimeBasedOTPProvider(IDateTimeProvider dateTimeProvider, bool disableCacheOtp)
        : this(dateTimeProvider)
    {
        _disableCacheOtp = disableCacheOtp;
    }

    public string GetPassword(string secret, int digits = 6, bool ignoreSpaces = false)
    {
        return _GetPassword(secret, GetCurrentCounter(), digits, ignoreSpaces);
    }

    public bool ValidateOTP(string password, 
                            string secret, 
                            bool ignoreSpaces = false, 
                            int checkAdjacentIntervals = 1)
    {
        if (_disableCacheOtp is false)
        {
            // Keeping a cache of the secret/password combinations that have been requested allows us to
            // make this a real one time use system. Once a secret/password combination has been tested,
            // it cannot be tested again until after it is no longer valid.
            // See http://tools.ietf.org/html/rfc6238#section-5.2 for more info.

            var cacheKey = $"{secret}_{password}";

            if (Cache.Contains(cacheKey))
            {
                throw new
                    OneTimePasswordException("You cannot use the same secret/iterationNumber combination more than once.");
            }

            // remove the cache item after 2 minutes
            Cache.Add(cacheKey,
                      cacheKey,
                      new CacheItemPolicy
                      {
                          SlidingExpiration = TimeSpan.FromMinutes(2)
                      });
        }

        if (password == GetPassword(secret, password.Length, ignoreSpaces))
            return true;

        for (int i = 1; i <= checkAdjacentIntervals; i++)
        {
            if (password == _GetPassword(secret, GetCurrentCounter() + i, password.Length, ignoreSpaces))
                return true;

            if (password == _GetPassword(secret, GetCurrentCounter() - i, password.Length, ignoreSpaces))
                return true;
        }

        return false;
    }

    private string _GetPassword(string secret, long counter, int digits = 6, bool ignoreSpaces = false)
    {
        return HashedOneTimePassword.GeneratePassword(secret, counter, digits, ignoreSpaces);
    }

    private long GetCurrentCounter()
    {
        return GetCurrentCounter(_dateTimeProvider.UtcNow, _dateTimeProvider.UnixEpoch);
    }

    private long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep = 30)
    {
        // default time-step size RECOMMENDED is 30 seconds
        // check out https://www.rfc-editor.org/rfc/rfc6238#section-5.2 for more information

        return (long)(now - epoch).TotalSeconds / timeStep;
    }
}