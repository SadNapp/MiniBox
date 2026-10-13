namespace MiniBox.Application.Common.Models;

public sealed class PaginatedList<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < PageCount;

    public static PaginatedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var allItems = source.ToArray();
        var totalCount = allItems.Length;
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return new PaginatedList<T>(items, page, pageSize, totalCount);
    }
}
