namespace BookCatalog.API.Dtos.Common;

public class PaginationQueryParameters
{
    private const int MaxLimit = 100;
    private int _pageIndex = 1;
    private int _limit = 20;
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? 1 : value;
    }
    public int Limit
    {
        get => _limit;
        set => _limit = value switch
        {
            < 1 => 20,
            > MaxLimit => MaxLimit,
            _ => value
        };
    }

}
