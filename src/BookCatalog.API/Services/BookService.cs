using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Entities;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.ExtensionMethods;
using BookCatalog.API.ExtensionMethods.Mapping;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services.Interfaces;
using BookCatalog.API.Utilities.Normalizers;
using BookCatalog.API.Utilities.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCatalog.API.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBaseRepository<Author> _authorRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IFileService _fileService;
    private readonly ILogger<BookService> _logger;
    public BookService(
        IBookRepository bookRepository,
        TimeProvider timeProvider,
        ILogger<BookService> logger,
        IBaseRepository<Author> authorRepository,
        ILoanRepository loanRepository,
        IFileService fileService)
    {
        _bookRepository = bookRepository;
        _timeProvider = timeProvider;
        _logger = logger;
        _authorRepository = authorRepository;
        _loanRepository = loanRepository;
        _fileService = fileService;
    }
    public async Task<Result<PagedList<BookDto>>> GetAllAsync(BookFilterQueryParameters paramters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all books.");

        var books = await _bookRepository.GetAll()
                                         .ApplyFilters(paramters)
                                         .Select(BookToDtoProjection)
                                         .ToPagedListAsync(paramters, ct);
        _logger.LogDebug("Retrieved {Count} books", books.Items.Count());

        return books;
    }
    public async Task<Result<BookDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving book with ID {BookId}.", id);

        var dto = await _bookRepository.GetAll()
                                       .Select(BookToDtoProjection)
                                       .FirstOrDefaultAsync(e => e.Id == id, ct);

        if(dto is null)
        {
            _logger.LogWarning("Book {BookId} was not found.", id);
            return Error.NotFound($"Book '{id}' was not found.");
        }

        return dto;
    }
    public async Task<Result<BookDto>> CreateAsync(CreateBookRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new book with Isbn {Isbn}", request.Isbn);

        var normalizedIsbn = IsbnNormalizer.Normalize(request.Isbn);
        if(await _bookRepository.IsIsbnTakenAsync(normalizedIsbn, ct))
        {
            _logger.LogWarning("Book with ISBN {Isbn} already exists.", request.Isbn);
            return Error.Conflict($"A different book with ISBN '{request.Isbn}' already exists.");
        }
        if(!await _authorRepository.ExistsAsync(request.AuthorId, ct))
        {
            _logger.LogWarning("No any author exists with this id {AuthorId}", request.AuthorId);
            return Error.NotFound($"No any author exists with this id {request.AuthorId}");
        }
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        if(request.PublicationDate > today)
        {
            _logger.LogWarning("Invalid publication date {PublicationDate}. It must be before or at {Today}.",
                request.PublicationDate, today);

            return Error.Validation([new(nameof(request.PublicationDate), $"Publication date must be before or at ({today})")]);
        }
        var uploadResult = await _fileService.UploadAsync(request.CoverPhoto, BlobCategory.CoverPhoto, ct);

        var book = request.ToEntity(normalizedIsbn, uploadResult.FileName);
        try
        {
            await _bookRepository.AddAsync(book, ct);
            await _bookRepository.SaveChangesAsync(ct);
            _logger.LogInformation("Created book {BookId}.", book.Id);
            return book.ToDto(uploadResult.FileURL);
        }
        catch
        {
            await TryDeleteBlobAsync(uploadResult.FileName, BlobCategory.CoverPhoto);
            throw;
        }

    }
    private async Task TryDeleteBlobAsync(string blobName, BlobCategory blobCategory)
    {
        try
        {
            await _fileService.DeleteAsync(blobName, blobCategory, CancellationToken.None);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to delete orphaned blob {BlobName}", blobName);
        }
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
        if(request.AuthorId != targetBook.AuthorId &&
            !await _authorRepository.ExistsAsync(request.AuthorId, ct))
        {
            _logger.LogWarning("No any author exists with this id {AuthorId}", request.AuthorId);
            return Error.NotFound($"No any author exists with this id {request.AuthorId}");
        }
        var normalizedIsbn = IsbnNormalizer.Normalize(request.Isbn);
        if(targetBook.NormalizedIsbn != normalizedIsbn &&
            await _bookRepository.IsIsbnTakenAsync(normalizedIsbn, ct))
        {
            _logger.LogWarning("Book update for {BookId} conflicts with ISBN {Isbn}.", id, request.Isbn);

            return Error.Conflict($"A different book with ISBN '{request.Isbn}' already exists.");
        }
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        if(request.PublicationDate > today)
        {
            _logger.LogWarning("Invalid publication date {PublicationDate}. It must be before or at {Today}.",
                request.PublicationDate, today);

            return Error.Validation([new(nameof(request.PublicationDate), $"Publication date must be before or at ({today})")]);
        }

        request.UpdateEntity(targetBook, normalizedIsbn);
        _bookRepository.Update(targetBook);
        await _bookRepository.SaveChangesAsync(ct);
        _logger.LogInformation("Updated book {BookId}.", id);

        return targetBook.ToDto(_fileService.GetURL(targetBook.CoverPhoto, BlobCategory.CoverPhoto));
    }

    public async Task<Result<BookDto>> UpdateCoverPhotoAsync(int id, UpdateCoverPhotoRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating cover photo for book with ID {BookId}.", id);

        var targetBook = await _bookRepository.GetByIdAsync(id, ct);
        if(targetBook is null)
        {
            _logger.LogWarning("Book {BookId} was not found.", id);
            return Error.NotFound($"Book '{id}' was not found.");
        }

        var oldPhotoName = targetBook.CoverPhoto;

        var uploadResult = await _fileService.UploadAsync(request.CoverPhoto, BlobCategory.CoverPhoto, ct);

        targetBook.CoverPhoto = uploadResult.FileName;
        _bookRepository.Update(targetBook);

        try
        {
            await _bookRepository.SaveChangesAsync(ct);
            _logger.LogInformation("Updated cover photo for book {BookId}.", id);
        }
        catch
        {
            await TryDeleteBlobAsync(uploadResult.FileName, BlobCategory.CoverPhoto);
            throw;
        }

        if(!string.IsNullOrEmpty(oldPhotoName))
        {
            await TryDeleteBlobAsync(oldPhotoName, BlobCategory.CoverPhoto);
        }

        return targetBook.ToDto(uploadResult.FileURL);
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

        if(await _loanRepository.BookHasActiveLoanAsync(id, ct))
        {
            _logger.LogWarning("Book {BookId} cannot be deleted because one of its copies has an active loan.", id);
            return Error.Conflict($"Book '{id}' cannot be deleted because one of its copies has an active loan.");
        }

        var photoName = book.CoverPhoto;

        _bookRepository.Delete(book);
        await _bookRepository.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted book {BookId}.", id);

        if(!string.IsNullOrEmpty(photoName))
        {
            await TryDeleteBlobAsync(photoName, BlobCategory.CoverPhoto);
        }

        return Result.Ok();
    }

    private Expression<Func<Book, BookDto>> BookToDtoProjection => book => new BookDto
    {
        Id = book.Id,
        AuthorId = book.AuthorId,
        Title = book.Title,
        Genre = book.Genre,
        Description = book.Description,
        Isbn = book.Isbn,
        Price = book.Price,
        PublicationDate = book.PublicationDate,
        CoverPhotoURL = _fileService.GetURL(book.CoverPhoto, BlobCategory.CoverPhoto)
    };
}
