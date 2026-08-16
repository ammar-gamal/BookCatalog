using BookCatalog.API.Dtos;
using BookCatalog.API.Entities;
using BookCatalog.API.Utilities;

namespace BookCatalog.API.ExtensionMethods.Mapping;

public static class BookMappingExtensions
{
    public static Book ToEntity(this CreateBookRequestDto dto)
        => new()
        {
            Author = dto.Author,
            Title = dto.Title,
            Genre = dto.Genre,
            Description = dto.Description,
            Isbn = dto.Isbn,
            Price = dto.Price,
            PublicationYear = dto.PublicationYear,
            NormalizedIsbn = IsbnNormalizer.Normalize(dto.Isbn)
        };
    public static BookDto ToDto(this Book book)
       => new()
       {
           Id = book.Id,
           Author = book.Author,
           Title = book.Title,
           Genre = book.Genre,
           Description = book.Description,
           Isbn = book.Isbn,
           Price = book.Price,
           PublicationYear = book.PublicationYear
       };
}
