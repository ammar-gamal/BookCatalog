using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Entities;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services;
using BookCatalog.API.Utilities.Normalizers;
using BookCatalog.API.Utilities.Results;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace BookCatalog.UnitTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILogger<BookService>> _loggerMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly BookService _sut;
    private readonly DateTime _fixedUtcNow = new(2026, 8, 20, 12, 0, 0);

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<BookService>>();
        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(_fixedUtcNow);

        _sut = new BookService(
            _bookRepositoryMock.Object,
            _timeProvider,
            _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenBookWithSameIsbnExists_ReturnsConflictError()
    {
        // Arrange
        var request = CreateValidCreateBookRequestDto();
        int existingBookId = 99;

        _bookRepositoryMock
            .Setup(r => r.GetBookIdByIsbnAsync(request.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBookId);

        // Act
        var result = await _sut.CreateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);

        _bookRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)] // Today
    [InlineData(10)] // Future
    public async Task CreateAsync_WhenPublicationYearIsTodayOrFuture_ReturnsValidationError(int numberOfDays)
    {
        // Arrange
        var request = CreateValidCreateBookRequestDto() with
        {
            PublicationYear = DateOnly.FromDateTime(_fixedUtcNow.AddDays(numberOfDays))
        };

        _bookRepositoryMock
            .Setup(r => r.GetBookIdByIsbnAsync(request.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        // Act
        var result = await _sut.CreateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);

        _bookRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_AllValid_AddsBookAndReturnsSuccessResult()
    {
        // Arrange
        var request = CreateValidCreateBookRequestDto();
        int expectedId = 1;

        _bookRepositoryMock
            .Setup(r => r.GetBookIdByIsbnAsync(request.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        _bookRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, _) => book.Id = expectedId)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(expectedId);

        _bookRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Book>(b =>
                b.Title == request.Title &&
                b.Author == request.Author &&
                b.Isbn == request.Isbn &&
                b.NormalizedIsbn == IsbnNormalizer.Normalize(request.Isbn) &&
                b.Price == request.Price &&
                b.Genre == request.Genre &&
                b.PublicationYear == request.PublicationYear &&
                b.Description == request.Description),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        int bookId = 99;
        var request = CreateValidUpdateBookRequestDto();

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.UpdateAsync(bookId, request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenIsbnBelongsToDifferentBook_ReturnsConflictError()
    {
        // Arrange
        int bookId = 1;
        int conflictingBookId = 999;
        var request = CreateValidUpdateBookRequestDto();
        var existingBook = CreateExistingBookEntity(bookId);

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(r => r.GetBookIdByIsbnAsync(request.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingBookId);

        // Act
        var result = await _sut.UpdateAsync(bookId, request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)] // Today
    [InlineData(10)] // Future
    public async Task UpdateAsync_WhenPublicationYearIsTodayOrFuture_ReturnsValidationError(int numberOfDays)
    {
        // Arrange
        int bookId = 1;
        var request = CreateValidUpdateBookRequestDto() with
        {
            PublicationYear = DateOnly.FromDateTime(_fixedUtcNow.AddDays(numberOfDays))
        };
        var existingBook = CreateExistingBookEntity(bookId);

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(r => r.GetBookIdByIsbnAsync(request.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookId);

        // Act
        var result = await _sut.UpdateAsync(bookId, request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Validation);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AllValid_UpdatesBookAndReturnsSuccessResult()
    {
        // Arrange
        int bookId = 1;
        var request = CreateValidUpdateBookRequestDto();
        var existingBook = CreateExistingBookEntity(bookId);

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(r => r.GetBookIdByIsbnAsync(request.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookId);

        _bookRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(bookId, request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(bookId);

        _bookRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Book>(b =>
                b.Id == bookId &&
                b.Title == request.Title &&
                b.Author == request.Author &&
                b.Isbn == request.Isbn &&
                b.NormalizedIsbn == IsbnNormalizer.Normalize(request.Isbn) &&
                b.Price == request.Price &&
                b.Genre == request.Genre &&
                b.PublicationYear == request.PublicationYear &&
                b.Description == request.Description),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenBookDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        int bookId = 99;

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.DeleteAsync(bookId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);

        _bookRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AllValid_DeletesBookAndReturnsSuccessResult()
    {
        // Arrange
        int bookId = 1;
        var existingBook = CreateExistingBookEntity(bookId);

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _bookRepositoryMock
            .Setup(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(bookId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _bookRepositoryMock.Verify(
            r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        int nonExistentBookId = 42;

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentBookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.GetByIdAsync(nonExistentBookId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_AllValid_ReturnsSuccessResultWithBookDto()
    {
        // Arrange
        int bookId = 1;
        var existingBook = CreateExistingBookEntity(bookId);

        _bookRepositoryMock
            .Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _sut.GetByIdAsync(bookId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(bookId);
        result.Data.Title.Should().Be(existingBook.Title);
        result.Data.Author.Should().Be(existingBook.Author);
        result.Data.Isbn.Should().Be(existingBook.Isbn);
        result.Data.Price.Should().Be(existingBook.Price);
        result.Data.Genre.Should().Be(existingBook.Genre);
        result.Data.PublicationYear.Should().Be(existingBook.PublicationYear);
        result.Data.Description.Should().Be(existingBook.Description);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenBooksExist_ReturnsCorrectlyMappedDtos()
    {
        // Arrange
        var book1 = new Book() { Id = 1, Title = "how to become sw engineer at akvelon :)", Author = "Ammar Gamal", Isbn = "111", NormalizedIsbn = "111", Price = 30m, Genre = BookGenre.Science, PublicationYear = new DateOnly(2026, 8, 1) };
        var books = new List<Book> { book1 };

        _bookRepositoryMock
            .Setup(r => r.GetAll())
            .Returns(books.AsQueryable());

        // Act
        var result = await _sut.GetAllAsync(new(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        var actualDto = result.Data.Items.Single();
        actualDto.Id.Should().Be(book1.Id);
        actualDto.Author.Should().Be(book1.Author);
        actualDto.Title.Should().Be(book1.Title);
        actualDto.Description.Should().Be(book1.Description);
        actualDto.Isbn.Should().Be(book1.Isbn);
        actualDto.Genre.Should().Be(book1.Genre);
        actualDto.Price.Should().Be(book1.Price);
        actualDto.PublicationYear.Should().Be(book1.PublicationYear);

    }

    [Fact]
    public async Task GetAllAsync_WhenFilterByGenre_ReturnsMatchedBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Id = 1, Title = "how to become sw engineer at akvelon :)", Author = "Ammar Gamal", Isbn = "111", NormalizedIsbn = "111", Price = 30m, Genre = BookGenre.Science, PublicationYear = new DateOnly(2026, 8, 1) },
            new() { Id = 2, Title = "how to become intern at akvelon :)", Author = "Ammar Gamal", Isbn = "222", NormalizedIsbn = "222", Price = 40m, Genre = BookGenre.Science, PublicationYear = new DateOnly(2026, 8, 10) },
            new() { Id = 3, Title = "how to pass sw interview at akvelon :)", Author = "Ammar Gamal", Isbn = "333", NormalizedIsbn = "333", Price = 20m, Genre = BookGenre.General, PublicationYear = new DateOnly(2026, 8, 20) },
            new() { Id = 4, Title = "how to pass the probation period at akvelon :)", Author = "Ammar Gamal", Isbn = "444", NormalizedIsbn = "444", Price = 35m, Genre = BookGenre.Fiction, PublicationYear = new DateOnly(2026, 8, 21) }

        };

        _bookRepositoryMock
            .Setup(r => r.GetAll())
            .Returns(books.AsQueryable());

        var parameters = new BookFilterQueryParameters
        {
            Genre = BookGenre.Science,
            PageIndex = 1,
            Limit = 10
        };

        // Act
        var result = await _sut.GetAllAsync(parameters, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
        result.Data.Items.Should().AllSatisfy(b =>
        {
            b.Genre.Should().Be(BookGenre.Science);
        });
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBooksMatchFilter_ReturnsEmptyPagedList()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Id = 1, Title = "how to become sw engineer at akvelon :)", Author = "Ammar Gamal", Isbn = "111", NormalizedIsbn = "111", Price = 30m, Genre = BookGenre.Science, PublicationYear = new DateOnly(2026, 8, 1) },
        };

        _bookRepositoryMock
            .Setup(r => r.GetAll())
            .Returns(books.AsQueryable());

        var parameters = new BookFilterQueryParameters
        {
            Genre = BookGenre.Fiction,
            PageIndex = 1,
            Limit = 10
        };

        // Act
        var result = await _sut.GetAllAsync(parameters, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalCount.Should().Be(0);
        result.Data.TotalPages.Should().Be(0);
        result.Data.Items.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private CreateBookRequestDto CreateValidCreateBookRequestDto() => new()
    {
        Title = ".NET 10",
        Author = "Ammar Gamal",
        Isbn = "10-10-10",
        Price = 99.99m,
        Genre = BookGenre.Science,
        PublicationYear = DateOnly.FromDateTime(_fixedUtcNow.AddDays(-10)),
        Description = "Intensive .NET"
    };

    private UpdateBookRequestDto CreateValidUpdateBookRequestDto() => new()
    {
        Title = ".NET 10 (2nd)",
        Author = "Ammar Gamal",
        Isbn = "20-20-20",
        Price = 119.99m,
        Genre = BookGenre.Science,
        PublicationYear = DateOnly.FromDateTime(_fixedUtcNow.AddDays(-5)),
        Description = "Updated Intensive .NET"
    };

    private Book CreateExistingBookEntity(int id) => new()
    {
        Id = id,
        Title = "How to become SW at Akvelon in 2026",
        Author = "Ammar Gamal",
        Isbn = "ISBN",
        NormalizedIsbn = "ISBN",
        Price = 50.00m,
        Genre = BookGenre.General,
        PublicationYear = DateOnly.FromDateTime(_fixedUtcNow.AddDays(-20)),
        Description = "Tips and Trick in interviews"
    };

    #endregion
}
