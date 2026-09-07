using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Entities.Enums;
using BookCatalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookCatalog.IntegrationTests.Books;

public class BookTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public BookTests(BookCatalogWebApplicationFactory factory) : base(factory)
    {
    }

    #region Creation & Data Integrity Tests

    [Fact]
    public async Task CreateBook_WithValidData_Returns201CreatedAndPersists()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange:
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));

        var request = new CreateBookRequestDto
        {
            AuthorId = author.Id,
            Title = "Refactoring",
            Isbn = "978-0201485677",
            Price = 49.99m,
            Genre = BookGenre.NonFiction,
            Description = "Improving the Design of Existing Code",
            PublicationDate = new DateOnly(1999, 7, 8)
        };

        // Act: 
        var response = await _client.PostAsJsonAsync("/api/books", request, _jsonOptions, ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<BookDto>(_jsonOptions, ct);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Title.Should().Be(request.Title);
        created.AuthorId.Should().Be(author.Id);
        created.Price.Should().Be(request.Price);
        created.Genre.Should().Be(request.Genre);

        // Verify database persistence via GET endpoint
        var getResponse = await _client.GetAsync($"/api/books/{created.Id}", ct);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<BookDto>(_jsonOptions, ct);
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task CreateBook_WithDuplicateIsbn_Returns409Conflict()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        var existingBook = await SeedAsync(s => s.SeedBookAsync(author.Id, isbn: "978-0-13-235088-4", ct: ct));

        // Act
        var duplicateRequest = new CreateBookRequestDto
        {
            AuthorId = author.Id,
            Title = "Different Title",
            Isbn = "9780132350884", // Same ISBN without hyphens
            Price = 29.99m,
            Genre = BookGenre.Fiction,
            PublicationDate = new DateOnly(2020, 1, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/books", duplicateRequest, _jsonOptions, ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBook_WithNonExistentAuthor_Returns404NotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = new CreateBookRequestDto
        {
            AuthorId = 999999,
            Title = "Orphan Book",
            Isbn = "978-1234567890",
            Price = 19.99m,
            Genre = BookGenre.Fiction,
            PublicationDate = new DateOnly(2022, 1, 1)
        };

        var response = await _client.PostAsJsonAsync("/api/books", request, _jsonOptions, ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_WithFuturePublicationDate_Returns400BadRequest()
    {
        var ct = TestContext.Current.CancellationToken;

        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));

        var request = new CreateBookRequestDto
        {
            AuthorId = author.Id,
            Title = "Future Book",
            Isbn = "978-9876543210",
            Price = 19.99m,
            Genre = BookGenre.Science,
            PublicationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        var response = await _client.PostAsJsonAsync("/api/books", request, _jsonOptions, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Retrieval & Query Operations

    [Fact]
    public async Task GetBookById_WhenBookExists_Returns200OkWithDetails()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var author = await SeedAsync(s => s.SeedAuthorAsync(name: "Eric Evans", ct: ct));
        var book = await SeedAsync(s => s.SeedBookAsync(
            author.Id,
            title: "Domain-Driven Design",
            price: 54.99m,
            genre: BookGenre.NonFiction,
            ct: ct));

        // Act
        var response = await _client.GetAsync($"/api/books/{book.Id}", ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<BookDto>(_jsonOptions, ct);
        dto.Should().NotBeNull();
        dto.Id.Should().Be(book.Id);
        dto.Title.Should().Be("Domain-Driven Design");
        dto.Price.Should().Be(54.99m);
        dto.Genre.Should().Be(BookGenre.NonFiction);
        dto.AuthorId.Should().Be(author.Id);
    }

    [Fact]
    public async Task GetBookById_WhenBookDoesNotExist_Returns404NotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync("/api/books/999999", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBooks_WithGenreAndPriceFilters_ReturnsMatchingBooksOnly()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange:
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        var book1 = await SeedAsync(s => s.SeedBookAsync(author.Id, title: "Cheap Fiction", price: 25.00m, genre: BookGenre.Fiction, ct: ct));
        var book2 = await SeedAsync(s => s.SeedBookAsync(author.Id, title: "Target Fiction", price: 55.00m, genre: BookGenre.Fiction, ct: ct));
        var book3 = await SeedAsync(s => s.SeedBookAsync(author.Id, title: "InRange NonFiction", price: 65.00m, genre: BookGenre.NonFiction, ct: ct));
        var book4 = await SeedAsync(s => s.SeedBookAsync(author.Id, title: "OutRange Fiction", price: 120.00m, genre: BookGenre.Fiction, ct: ct));


        // Act
        var response = await _client.GetAsync("/api/books?genre=Fiction&priceFrom=30&priceEnd=70", ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedList<BookDto>>(_jsonOptions, ct);
        paged.Should().NotBeNull();
        paged.Items.Should().ContainSingle();
        paged.Items.First().Id.Should().Be(book2.Id);
    }

    [Fact]
    public async Task GetBooks_WithSortingByPriceDesc_ReturnsBooksInDescendingPriceOrder()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange:
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        await SeedAsync(s => s.SeedBookAsync(author.Id, title: "Cheap", price: 10.00m, ct: ct));
        await SeedAsync(s => s.SeedBookAsync(author.Id, title: "Expensive", price: 90.00m, ct: ct));
        await SeedAsync(s => s.SeedBookAsync(author.Id, title: "Medium", price: 40.00m, ct: ct));

        // Act:
        var response = await _client.GetAsync("/api/books?sortBy=Price&sortDir=Desc", ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedList<BookDto>>(_jsonOptions, ct);
        paged.Should().NotBeNull();
        var prices = paged.Items.Select(b => b.Price).ToList();
        prices.Should().BeInDescendingOrder();
        prices.First().Should().Be(90.00m);
    }

    [Fact]
    public async Task GetBooks_WithPagination_ReturnsCorrectPageAndMetadata()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange: 
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        for(int i = 1; i <= 5; i++)
        {
            await SeedAsync(s => s.SeedBookAsync(author.Id, title: $"Paging_Book_{i}", ct: ct));
        }

        // Act: 
        var response = await _client.GetAsync("/api/books?pageIndex=1&limit=2", ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedList<BookDto>>(_jsonOptions, ct);
        paged.Should().NotBeNull();
        paged.TotalCount.Should().Be(5);
        paged.TotalPages.Should().Be(3);
        paged.PageIndex.Should().Be(1);
        paged.HasNext.Should().BeTrue();
        paged.HasPrevious.Should().BeFalse();
        paged.Items.Count().Should().Be(2);
        paged.Items.Select(x => x.Title).Should()//Default ordering is by Id
                   .ContainInOrder("Paging_Book_1", "Paging_Book_2");
    }

    #endregion

    #region Update Operations

    [Fact]
    public async Task UpdateBook_WithValidData_Returns200OkAndPersistsChanges()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        var book = await SeedAsync(s => s.SeedBookAsync(author.Id, title: "Original Title", price: 20.00m, ct: ct));

        var updateRequest = new UpdateBookRequestDto
        {
            AuthorId = author.Id,
            Title = "Updated Title",
            Isbn = book.Isbn,
            Price = 45.00m,
            Genre = BookGenre.History,
            Description = "Updated Description",
            PublicationDate = book.PublicationDate
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/books/{book.Id}", updateRequest, _jsonOptions, ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<BookDto>(_jsonOptions, ct);
        updated.Should().NotBeNull();
        updated.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(45.00m);
        updated.Genre.Should().Be(BookGenre.History);

        // Verify persistence
        var getResponse = await _client.GetAsync($"/api/books/{book.Id}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<BookDto>(_jsonOptions, ct);
        fetched.Should().NotBeNull();
        fetched.Title.Should().Be("Updated Title");
        fetched.Genre.Should().Be(BookGenre.History);
        fetched.Price.Should().Be(45.00m);
    }

    [Fact]
    public async Task UpdateBook_WithConflictingIsbnFromAnotherBook_Returns409Conflict()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        var bookA = await SeedAsync(s => s.SeedBookAsync(author.Id, isbn: "978-1111111111", ct: ct));
        var bookB = await SeedAsync(s => s.SeedBookAsync(author.Id, isbn: "978-2222222222", ct: ct));

        // Act
        var updateRequest = new UpdateBookRequestDto
        {
            AuthorId = author.Id,
            Title = bookB.Title,
            Isbn = bookA.Isbn, // Conflicting ISBN
            Price = bookB.Price,
            Genre = bookB.Genre,
            PublicationDate = bookB.PublicationDate
        };

        var response = await _client.PutAsJsonAsync($"/api/books/{bookB.Id}", updateRequest, _jsonOptions, ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateBook_WhenBookNotFound_Returns404NotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));

        var updateRequest = new UpdateBookRequestDto
        {
            AuthorId = author.Id,
            Title = "NonExistent Book",
            Isbn = "978-9999999999",
            Price = 25.00m,
            Genre = BookGenre.Fiction,
            PublicationDate = new DateOnly(2020, 1, 1)
        };

        var response = await _client.PutAsJsonAsync("/api/books/999999", updateRequest, _jsonOptions, ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete & Deletion Protection Operations

    [Fact]
    public async Task DeleteBook_WhenNoActiveLoans_Returns204NoContentAndRemovesFromDb()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var author = await SeedAsync(s => s.SeedAuthorAsync(ct: ct));
        var book = await SeedAsync(s => s.SeedBookAsync(author.Id, ct: ct));

        // Act
        var response = await _client.DeleteAsync($"/api/books/{book.Id}", ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify book is removed from database
        var getResponse = await _client.GetAsync($"/api/books/{book.Id}", ct);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_WhenBookHasActiveLoan_Returns409Conflict()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var seeded = await SeedAsync(s => s.SeedCompleteActiveLoanForBookAsync(ct));

        // Act
        var response = await _client.DeleteAsync($"/api/books/{seeded.Book.Id}", ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Verify the book still remains intact
        var getResponse = await _client.GetAsync($"/api/books/{seeded.Book.Id}", ct);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
