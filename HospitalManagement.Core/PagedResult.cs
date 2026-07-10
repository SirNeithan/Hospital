namespace HospitalManagement.Core;

/// <summary>Non-generic interface so view components can accept any PagedResult&lt;T&gt;.</summary>
public interface IPaged
{
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize   { get; }
    int TotalPages { get; }
    bool HasPrev   { get; }
    bool HasNext   { get; }
    int From       { get; }
    int To         { get; }
}

/// <summary>
/// A generic container that holds one page of query results plus
/// all the metadata needed to render a pagination control.
/// Services return IQueryable; the page model materialises it into
/// a PagedResult so the database only fetches the rows we need.
/// </summary>
public class PagedResult<T> : IPaged
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrev   => PageNumber > 1;
    public bool HasNext   => PageNumber < TotalPages;
    public int  From      => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
    public int  To        => Math.Min(PageNumber * PageSize, TotalCount);

    /// <summary>
    /// Materialises an IQueryable into a PagedResult.
    /// The COUNT and the slice are two separate SQL queries.
    /// </summary>
    public static PagedResult<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize   = Math.Clamp(pageSize, 1, 100);

        var total = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items       = items,
            TotalCount  = total,
            PageNumber  = pageNumber,
            PageSize    = pageSize
        };
    }
}
