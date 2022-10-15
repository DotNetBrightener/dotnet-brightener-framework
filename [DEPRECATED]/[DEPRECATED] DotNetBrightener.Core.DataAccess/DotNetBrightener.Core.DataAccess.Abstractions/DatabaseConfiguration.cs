namespace DotNetBrightener.Core.DataAccess.Abstractions
{
    public class DatabaseConfiguration
    {
        public string ConnectionString { get; set; }

        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.MsSql;

        public bool UseLazyLoadingProxies { get; set; } = true;
    }
}