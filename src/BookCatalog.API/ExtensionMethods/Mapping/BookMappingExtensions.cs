using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Entities;

namespace BookCatalog.API.ExtensionMethods.Mapping;

public static class BookMappingExtensions
{
    public static Book ToEntity(this CreateBookRequestDto dto, string normalizedIsbn)
        => new()
        {
            AuthorId = dto.AuthorId,
            Title = dto.Title,
            Genre = dto.Genre,
            Description = dto.Description,
            Isbn = dto.Isbn,
            Price = dto.Price,
            PublicationDate = dto.PublicationDate,
            NormalizedIsbn = normalizedIsbn,
        };
    public static void UpdateEntity(this UpdateBookRequestDto request, Book book, string normalizedIsbn)
    {
        book.Title = request.Title;
        book.AuthorId = request.AuthorId;
        book.Isbn = request.Isbn;
        book.PublicationDate = request.PublicationDate;
        book.Price = request.Price;
        book.Description = request.Description;
        book.Genre = request.Genre;
        book.NormalizedIsbn = normalizedIsbn;
    }
    public static BookDto ToDto(this Book book)
       => new()
       {
           Id = book.Id,
           AuthorId = book.AuthorId,
           Title = book.Title,
           Genre = book.Genre,
           Description = book.Description,
           Isbn = book.Isbn,
           Price = book.Price,
           PublicationDate = book.PublicationDate
       };

}
