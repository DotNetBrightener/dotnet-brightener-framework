namespace DotNetBrightener.GenericCRUD.Models;

public class PagedCollection<T>
{
    public IEnumerable<T> Items { get; set; }

    public int TotalCount { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int ResultCount { get; set; }
}

public class PagedCollection : PagedCollection<dynamic>;

public class Pagination
{
    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}