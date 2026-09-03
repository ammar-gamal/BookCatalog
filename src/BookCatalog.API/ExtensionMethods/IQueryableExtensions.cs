using BookCatalog.API.Dtos.Common;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.ExtensionMethods;

public static class IQueryableExtensions
{
    public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, PaginationQueryParameters parameters)
    {
        int limit = parameters.Limit;
        int index = parameters.PageIndex;
        int count = source.Count();
        int totalPages = (int)Math.Ceiling((double)count / limit);

        if(totalPages <= 0)
            return new PagedList<T>([], 0, 0, 0);

        if(index > totalPages)
            index = totalPages;

        var items = source.Skip((index - 1) * limit)
                          .Take(limit)
                          .ToList();

        return new PagedList<T>(
            items: items,
            totalPages: totalPages,
            totalCount: count,
            pageIndex: index);
    }
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        int limit = parameters.Limit;
        int index = parameters.PageIndex;
        int count = await source.CountAsync(ct);
        int totalPages = (int)Math.Ceiling((double)count / limit);

        if(totalPages <= 0)
            return new PagedList<T>([], 0, 0, 0);

        if(index > totalPages)
            index = totalPages;

        var items = await source.Skip((index - 1) * limit)
                          .Take(limit)
                          .ToListAsync(ct);

        return new PagedList<T>(
            items: items,
            totalPages: totalPages,
            totalCount: count,
            pageIndex: index);
    }
}
