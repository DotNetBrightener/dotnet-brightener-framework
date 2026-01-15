using Microsoft.Extensions.Logging;

namespace DotNetBrightener.SimpleUploadService.Utils;

/// <summary>
///     Manages file caching operations with intelligent disk space management
/// </summary>
internal static class FileCacheManager
{
    /// <summary>
    ///     Saves a stream to cache file with automatic space management
    /// </summary>
    /// <param name="sourceStream">Stream to save</param>
    /// <param name="cacheFilePath">Full path to cache file</param>
    /// <param name="cacheDirectory">Cache directory path</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if saved successfully, false otherwise</returns>
    public static async Task<bool> TrySaveToCacheAsync(Stream            sourceStream,
                                                       string            cacheFilePath,
                                                       string            cacheDirectory,
                                                       ILogger           logger,
                                                       CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureCacheDirectoryExists(cacheDirectory, logger);

            await using (var fileStream = File.Create(cacheFilePath))
            {
                await sourceStream.CopyToAsync(fileStream, cancellationToken);
            }

            logger.LogInformation("Successfully cached file: {cacheFilePath} ({fileSize:N0} bytes)",
                                  cacheFilePath,
                                  new FileInfo(cacheFilePath).Length);

            return true;
        }
        catch (IOException ex) when (IsDiskSpaceException(ex))
        {
            logger.LogWarning("Insufficient disk space to cache file {cacheFilePath}. Attempting cache eviction...",
                              cacheFilePath);

            return await TrySaveWithEvictionAsync(sourceStream,
                                                  cacheFilePath,
                                                  cacheDirectory,
                                                  logger,
                                                  cancellationToken);
        }
        catch (IOException ex) when (ex.Message.Contains("being used by another process"))
        {
            logger.LogDebug("Cache file {cacheFilePath} is being written by another request (race condition).",
                            cacheFilePath);

            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                              "Failed to cache file {cacheFilePath}. File will be served without caching.",
                              cacheFilePath);

            return false;
        }
    }

    /// <summary>
    /// Attempts to save file after evicting old cache entries
    /// </summary>
    private static async Task<bool> TrySaveWithEvictionAsync(Stream            sourceStream,
                                                             string            cacheFilePath,
                                                             string            cacheDirectory,
                                                             ILogger           logger,
                                                             CancellationToken cancellationToken)
    {
        try
        {
            long requiredBytes = sourceStream.CanSeek
                                     ? sourceStream.Length
                                     : 10 * 1024 * 1024;

            requiredBytes = (long)(requiredBytes * 1.1);

            logger.LogInformation("Attempting to free {requiredBytes:N0} bytes from cache directory: {cacheDirectory}",
                                  requiredBytes,
                                  cacheDirectory);

            var freedBytes = EvictOldestCacheFiles(cacheDirectory, requiredBytes, logger);

            if (freedBytes < requiredBytes)
            {
                logger.LogWarning("Could only free {freedBytes:N0} bytes, needed {requiredBytes:N0} bytes. Cache save may still fail.",
                                  freedBytes,
                                  requiredBytes);
            }

            if (sourceStream.CanSeek)
            {
                sourceStream.Position = 0;
            }
            else
            {
                logger.LogWarning("Cannot retry cache save - stream is not seekable");

                return false;
            }

            await using (var fileStream = File.Create(cacheFilePath))
            {
                await sourceStream.CopyToAsync(fileStream, cancellationToken);
            }

            logger.LogInformation("Successfully cached file after eviction: {cacheFilePath} ({fileSize:N0} bytes)",
                                  cacheFilePath,
                                  new FileInfo(cacheFilePath).Length);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Failed to cache file {cacheFilePath} even after eviction attempt.",
                            cacheFilePath);

            return false;
        }
    }

    /// <summary>
    /// Evicts oldest cache files until required space is freed
    /// </summary>
    /// <param name="cacheDirectory">Cache directory path</param>
    /// <param name="requiredBytes">Bytes to free</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>Total bytes freed</returns>
    private static long EvictOldestCacheFiles(string cacheDirectory, long requiredBytes, ILogger logger)
    {
        try
        {
            if (!Directory.Exists(cacheDirectory))
            {
                return 0;
            }

            var files = new DirectoryInfo(cacheDirectory)
                       .GetFiles()
                       .OrderBy(f => f.LastAccessTimeUtc)
                       .ThenBy(f => f.LastWriteTimeUtc)
                       .ToList();

            if (files.Count == 0)
            {
                logger.LogWarning("No cache files available for eviction in {cacheDirectory}", cacheDirectory);

                return 0;
            }

            long totalFreed   = 0;
            var  evictedFiles = new List<string>();

            foreach (var file in files)
            {
                if (totalFreed >= requiredBytes)
                {
                    break;
                }

                try
                {
                    var fileSize = file.Length;
                    file.Delete();

                    totalFreed += fileSize;
                    evictedFiles.Add(file.Name);

                    logger.LogDebug("Evicted cache file: {fileName} ({fileSize:N0} bytes)",
                                    file.Name,
                                    fileSize);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to evict cache file: {fileName}", file.Name);
                }
            }

            logger.LogInformation("Cache eviction completed: freed {totalFreed:N0} bytes by removing {count} file(s)",
                                  totalFreed,
                                  evictedFiles.Count);

            return totalFreed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during cache eviction in {cacheDirectory}", cacheDirectory);

            return 0;
        }
    }

    /// <summary>
    /// Ensures cache directory exists, creates if necessary
    /// </summary>
    public static void EnsureCacheDirectoryExists(string cacheDirectory, ILogger logger)
    {
        try
        {
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
                logger.LogInformation("Created cache directory: {cacheDirectory}", cacheDirectory);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create cache directory: {cacheDirectory}", cacheDirectory);

            throw;
        }
    }

    /// <summary>
    /// Checks if exception is related to disk space
    /// </summary>
    private static bool IsDiskSpaceException(IOException ex)
    {
        const int ERROR_DISK_FULL        = 0x70;
        const int ERROR_HANDLE_DISK_FULL = 0x27;

        var hResult = ex.HResult & 0xFFFF;

        return hResult == ERROR_DISK_FULL ||
               hResult == ERROR_HANDLE_DISK_FULL ||
               ex.Message.Contains("disk", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("space", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates cache file and checks expiration
    /// </summary>
    /// <param name="cacheFilePath">Path to cache file</param>
    /// <param name="cacheExpiration">Cache expiration timespan</param>
    /// <returns>True if cache is valid and not expired</returns>
    public static bool IsCacheValid(string cacheFilePath, TimeSpan cacheExpiration)
    {
        if (!File.Exists(cacheFilePath))
        {
            return false;
        }

        var fileInfo = new FileInfo(cacheFilePath);

        return DateTime.UtcNow - fileInfo.LastWriteTimeUtc < cacheExpiration;
    }
}