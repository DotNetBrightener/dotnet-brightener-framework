namespace DotNetBrightener.UploadService.AzureBlobStorage;

public class AzureBlobStorageConfiguration
{
    public const string DefaultDownloadFolder = "tmp_download";

    public const string DefaultFileEndpoint = "/azure_file";

    public string ConnectionString { get; set; }

    public string RetrieveFileEndpoint { get; set; } = DefaultFileEndpoint;

    public bool UploadInBackground { get; set; }

    public string TempDownloadFolder { get; set; } = DefaultDownloadFolder;

    public bool UseGuidForFileName { get; set; }

    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(6);
}