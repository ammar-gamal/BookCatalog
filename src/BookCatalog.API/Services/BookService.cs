using BookCatalog.API.Dtos;
using BookCatalog.API.ExtensionMethods.Mapping;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services.Interfaces;
using BookCatalog.API.Utilities;

namespace BookCatalog.API.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BookService> _logger;
    public BookService(IBookRepository bookRepository, TimeProvider timeProvider, ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _timeProvider = timeProvider;
        _logger = logger;
    }
    public Task<Result<List<BookDto>>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all books.");

        var books = _bookRepository.GetAll()
                                   .Select(b => new BookDto()
                                   {
                                       Id = b.Id,
                                       Author = b.Author,
                                       Title = b.Title,
                                       Genre = b.Genre,
                                       Description = b.Description,
                                       Isbn = b.Isbn,
                                       Price = b.Price,
                                       PublicationYear = b.PublicationYear
                                   })
                                   .ToList();
        _logger.LogDebug("Retrieved {Count} books", books.Count);

        return Task.FromResult(Result<List<BookDto>>.Ok(books));
    }

    public async Task<Result<BookDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving book with ID {BookId}.", id);

        var book = await _bookRepository.GetByIdAsync(id, ct);

        if(book is null)
        {
            _logger.LogWarning("Book {BookId} was not found.", id);
            return Error.NotFound($"Book '{id}' was not found.");
        }

        return book.ToDto();
    }

    public async Task<Result<BookDto>> CreateAsync(CreateBookRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new book with Isbn {Isbn}", request.Isbn);

        if(await _bookRepository.GetBookIdByIsbnAsync(request.Isbn, ct) is int)
        {
            _logger.LogWarning("Book with ISBN {Isbn} already exists.", request.Isbn);
            return Error.Conflict($"A different book with ISBN '{request.Isbn}' already exists.");
        }
        var utcToday = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        if(request.PublicationYear >= utcToday)
        {
            _logger.LogWarning("Invalid publication year {PublicationYear}. It must be before {Today}.",
                request.PublicationYear, utcToday);

            return Error.Validation([new(nameof(request.PublicationYear), $"Publication year must be before today ({utcToday})")]);
        }

        var book = request.ToEntity();
        await _bookRepository.AddAsync(book, ct);
        _logger.LogInformation("Created book {BookId}.", book.Id);

        return book.ToDto();
    }

    public async Task<Result<BookDto>> UpdateAsync(int id, UpdateBookRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating book with ID {BookId}.", id);

        var targetBook = await _bookRepository.GetByIdAsync(id, ct);

        if(targetBook is null)
        {
            _logger.LogWarning("Book {BookId} was not found.", id);
            return Error.NotFound($"Book '{id}' was not found.");
        }

        if(await _bookRepository.GetBookIdByIsbnAsync(request.Isbn, ct) is int bookId && bookId != id)
        {
            _logger.LogWarning("Book update for {BookId} conflicts with ISBN {Isbn}.", id, request.Isbn);

            return Error.Conflict($"A different book with ISBN '{request.Isbn}' already exists.");
        }
        var utcToday = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        if(request.PublicationYear >= utcToday)
        {
            _logger.LogWarning("Invalid publication year {PublicationYear}. It must be before {Today}.",
                request.PublicationYear, utcToday);

            return Error.Validation([new(nameof(request.PublicationYear), $"Publication year must be before today ({utcToday})")]);
        }

        targetBook.Title = request.Title;
        targetBook.Author = request.Author;
        targetBook.Isbn = request.Isbn;
        targetBook.NormalizedIsbn = IsbnNormalizer.Normalize(request.Isbn);
        targetBook.PublicationYear = request.PublicationYear;
        targetBook.Price = request.Price;
        targetBook.Description = request.Description;
        targetBook.Genre = request.Genre;
        await _bookRepository.UpdateAsync(targetBook, ct);
        _logger.LogInformation("Updated book {BookId}.", id);

        return targetBook.ToDto();
    }
    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting book with ID {BookId}.", id);
        var book = await _bookRepository.GetByIdAsync(id, ct);

        if(book is null)
        {
            _logger.LogWarning("Book {BookId} was not found for delete.", id);
            return Error.NotFound($"Book '{id}' was not found.");
        }

        await _bookRepository.DeleteAsync(id, ct);
        _logger.LogInformation("Deleted book {BookId}.", id);
        return Result.Ok();
    }
}
