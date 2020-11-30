namespace DotNetBrightener.MultiTenancy
{
    public interface ITenant
    {
        long Id { get; set; }

        string Name { get; set; }

        string TenantGuid { get; set; }

        string Domains { get; set; }

        bool IsActive { get; set; }
    }
}