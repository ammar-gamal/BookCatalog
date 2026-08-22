using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Entities.Enums;

namespace BookCatalog.API.Dtos.Book;

public class BookFilterQueryParameters : PaginationQueryParameters
{
    public BookGenre? Genre { get; set; }
    public DateOnly? PublicationYearFrom { get; set; }
    public DateOnly? PublicationYearEnd { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceEnd { get; set; }
    public BookSortField? SortBy { get; set; }
    public SortDirection? SortDir { get; set; }
}
public enum BookSortField
{
    Price,
    PublicationYear,
    Genre
}
public enum SortDirection
{
    Asc,
    Desc
}