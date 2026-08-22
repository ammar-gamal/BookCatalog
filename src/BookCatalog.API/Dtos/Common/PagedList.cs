namespace BookCatalog.API.Dtos.Common;

public class PagedList<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageIndex { get; }
    public bool HasNext => TotalPages > PageIndex;
    public bool HasPrevious => PageIndex > 1;
    public PagedList(IEnumerable<T> items, int totalPages, int totalCount, int pageIndex)
    {
        Items = items;
        TotalPages = totalPages;
        TotalCount = totalCount;
        PageIndex = pageIndex;
    }


}
