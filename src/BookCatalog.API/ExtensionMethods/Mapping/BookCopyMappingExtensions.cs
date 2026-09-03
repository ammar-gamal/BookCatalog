using BookCatalog.API.Dtos.BookCopy;
using BookCatalog.API.Entities;

namespace BookCatalog.API.ExtensionMethods.Mapping;

public static class BookCopyMappingExtensions
{
    public static BookCopy ToEntity(this UpsertBookCopyRequestDto dto)
        => new()
        {
            Barcode = dto.Barcode,
            BookId = dto.BookId
        };

    public static BookCopyDto ToDto(this BookCopy copy)
        => new()
        {
            Id = copy.Id,
            Barcode = copy.Barcode,
            BookId = copy.BookId
        };
}
