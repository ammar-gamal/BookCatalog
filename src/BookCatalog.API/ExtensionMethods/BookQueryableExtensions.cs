using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Entities;

namespace BookCatalog.API.ExtensionMethods;

public static class BookQueryableExtensions
{
    public static IQueryable<Book> ApplyFilters(this IQueryable<Book> source, BookFilterQueryParameters filterQuery)
    {
        if(filterQuery.Genre.HasValue)
            source = source.Where(t => t.Genre == filterQuery.Genre);
        if(filterQuery.PublicationYearFrom.HasValue)
            source = source.Where(t => t.PublicationDate >= filterQuery.PublicationYearFrom.Value);
        if(filterQuery.PublicationYearEnd.HasValue)
            source = source.Where(t => t.PublicationDate <= filterQuery.PublicationYearEnd.Value);
        if(filterQuery.PriceFrom.HasValue)
            source = source.Where(t => t.Price >= filterQuery.PriceFrom.Value);
        if(filterQuery.PriceEnd.HasValue)
            source = source.Where(t => t.Price <= filterQuery.PriceEnd.Value);

        var sortDir = filterQuery.SortDir;
        source = filterQuery.SortBy switch
        {
            BookSortField.Price => sortDir is SortDirection.Desc ? source.OrderByDescending(t => t.Price)
                                                                : source.OrderBy(t => t.Price),
            BookSortField.Genre => sortDir is SortDirection.Desc ? source.OrderByDescending(t => t.Genre)
                                                                : source.OrderBy(t => t.Genre),
            BookSortField.PublicationYear => sortDir is SortDirection.Desc ? source.OrderByDescending(t => t.PublicationDate)
                                                                          : source.OrderBy(t => t.PublicationDate),
            _ => source.OrderBy(e => e.Id)
        };

        return source;
    }
}
