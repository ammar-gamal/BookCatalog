using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Entities;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.ExtensionMethods;
using FluentAssertions;
using Xunit;

namespace BookCatalog.UnitTests.ExtensionMethods;

public class BookQueryableExtensionsTests
{
    private readonly List<Book> _sampleBooks =
    [
        new() { Id = 1, Title = "A", Author = "Author One", Genre = BookGenre.Science, Price = 10m, PublicationYear = new DateOnly(2010, 1, 1), Isbn = "1", NormalizedIsbn = "1" },
        new() { Id = 3, Title = "C", Author = "Author Two", Genre = BookGenre.History, Price = 20m, PublicationYear = new DateOnly(2015, 1, 1), Isbn = "3", NormalizedIsbn = "3" },
        new() { Id = 2, Title = "B", Author = "Author One", Genre = BookGenre.Fiction, Price = 30m, PublicationYear = new DateOnly(2020, 1, 1), Isbn = "2", NormalizedIsbn = "2" }
    ];

    [Fact]
    public void ApplyFilters_WhenGenreFilterProvided_ReturnsMatchedBooks()
    {
        // Arrange
        var filter = new BookFilterQueryParameters { Genre = BookGenre.Fiction };
        int expectedId = 2;

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(expectedId);
    }

    [Fact]
    public void ApplyFilters_WhenDateRangeProvided_ReturnsBooksWithinRange()
    {
        // Arrange
        var filter = new BookFilterQueryParameters
        {
            PublicationYearFrom = new DateOnly(2012, 1, 1),
            PublicationYearEnd = new DateOnly(2018, 1, 1)
        };
        int expectedId = 3;

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(expectedId);
    }

    [Fact]
    public void ApplyFilters_WhenPriceRangeProvided_ReturnsBooksWithinRange()
    {
        // Arrange
        var filter = new BookFilterQueryParameters
        {
            PriceFrom = 25m,
            PriceEnd = 35m
        };
        int expectedId = 2;

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(expectedId);
    }

    [Theory]
    [InlineData(SortDirection.Asc, new[] { 1, 3, 2 })]
    [InlineData(SortDirection.Desc, new[] { 2, 3, 1 })]
    public void ApplyFilters_WhenSortByPrice_ShouldSortsCorrectly(SortDirection direction, int[] expectedIdOrder)
    {
        // Arrange
        var filter = new BookFilterQueryParameters
        {
            SortBy = BookSortField.Price,
            SortDir = direction
        };

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Select(b => b.Id).Should().Equal(expectedIdOrder);
    }

    [Theory]
    [InlineData(SortDirection.Asc, new[] { 1, 3, 2 })]
    [InlineData(SortDirection.Desc, new[] { 2, 3, 1 })]
    public void ApplyFilters_WhenSortByPublicationYear_ShouldSortsCorrectly(SortDirection direction, int[] expectedIdOrder)
    {
        // Arrange
        var filter = new BookFilterQueryParameters
        {
            SortBy = BookSortField.PublicationYear,
            SortDir = direction
        };

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Select(b => b.Id).Should().Equal(expectedIdOrder);
    }

    //enum values: fiction=0, nonfiction=1, science=2, history=3, general=4

    [Theory]
    [InlineData(SortDirection.Asc, new[] { 2, 1, 3 })]
    [InlineData(SortDirection.Desc, new[] { 3, 1, 2 })]
    public void ApplyFilters_WhenSortByGenre_ShouldSortsCorrectly(SortDirection direction, int[] expectedIdOrder)
    {
        // Arrange
        var filter = new BookFilterQueryParameters
        {
            SortBy = BookSortField.Genre,
            SortDir = direction
        };

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Select(b => b.Id).Should().Equal(expectedIdOrder);
    }
    [Fact]
    public void ApplyFilters_WhenNoFiltersProvided_ReturnsAllBooks()
    {
        // Arrange
        var filter = new BookFilterQueryParameters();

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Should().HaveCount(3);
    }
    [Fact]
    public void ApplyFilters_WhenNoSortSpecified_ShouldSortByIdAscending()
    {
        // Arrange
        var filter = new BookFilterQueryParameters();

        // Act
        var result = _sampleBooks.AsQueryable().ApplyFilters(filter).ToList();

        // Assert
        result.Select(b => b.Id).Should().Equal(1, 2, 3);
    }
}
