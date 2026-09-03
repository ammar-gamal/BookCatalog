using BookCatalog.API.Dtos.Author;
using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Entities;

namespace BookCatalog.API.ExtensionMethods.Mapping;

public static class AuthorMappingExtensions
{
    public static Author ToEntity(this CreateAuthorRequestDto dto)
        => new()
        {
            Name = dto.Name,
            Biography = dto.Biography
        };
    public static AuthorDto ToDto(this Author author)
       => new()
       {
           Id = author.Id,
           Biography = author.Biography,
           Name = author.Name
       };
}
