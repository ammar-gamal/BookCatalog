using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Entities;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.ExtensionMethods.Mapping;
using BookCatalog.API.Utilities.Normalizers;
using FluentAssertions;
using Xunit;

namespace BookCatalog.UnitTests.ExtensionMethods;

public class BookMappingExtensionsTests
{
    [Fact]
    public void ToEntity_WhenGivenValidCreateBookRequestDto_MapsAllPropertiesToBook()
    {
        // Arrange
        var dto = new CreateBookRequestDto
        {
            Title = "Book Title1",
            Author = "Book Author",
            Isbn = "  2222-2222  ",
            Price = 14.99m,
            Genre = BookGenre.Fiction,
            PublicationYear = new DateOnly(2000, 2, 2),
            Description = "Book Description"
        };

        // Act
        var result = dto.ToEntity();

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.Author.Should().Be(dto.Author);
        result.Isbn.Should().Be(dto.Isbn);
        result.NormalizedIsbn.Should().Be(IsbnNormalizer.Normalize(dto.Isbn));
        result.Price.Should().Be(dto.Price);
        result.Genre.Should().Be(dto.Genre);
        result.PublicationYear.Should().Be(dto.PublicationYear);
        result.Description.Should().Be(dto.Description);
    }

    [Fact]
    public void ToDto_WhenGivenValidBookEntity_MapsAllPropertiesToBookDto()
    {
        // Arrange
        var entity = new Book
        {
            Id = 1,
            Title = "Book Title1",
            Author = "Book Author",
            Isbn = "2222-2222",
            NormalizedIsbn = "2222-2222",
            Price = 9.99m,
            Genre = BookGenre.Fiction,
            PublicationYear = new DateOnly(2000, 2, 2),
            Description = "Book Description"
        };

        // Act
        var result = entity.ToDto();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Title.Should().Be(entity.Title);
        result.Author.Should().Be(entity.Author);
        result.Isbn.Should().Be(entity.Isbn);
        result.Price.Should().Be(entity.Price);
        result.Genre.Should().Be(entity.Genre);
        result.PublicationYear.Should().Be(entity.PublicationYear);
        result.Description.Should().Be(entity.Description);
    }
}
