using BookCatalog.API.Dtos.BookCopy;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Entities;
using BookCatalog.API.ExtensionMethods;
using BookCatalog.API.ExtensionMethods.Mapping;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services.Interfaces;
using BookCatalog.API.Utilities.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCatalog.API.Services;

public class BookCopyService : IBookCopyService
{
    private readonly IBookCopyRepository _bookCopyRepository;
    private readonly IBaseRepository<Book> _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<BookCopyService> _logger;

    public BookCopyService(
        IBookCopyRepository bookCopyRepository,
        ILoanRepository loanRepository,
        IBaseRepository<Book> bookRepository,
        ILogger<BookCopyService> logger)
    {
        _bookCopyRepository = bookCopyRepository;
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
        _logger = logger;
    }
    public async Task<Result<PagedList<BookCopyDto>>> GetAllAsync(PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all book copies.");

        var copies = await _bookCopyRepository.GetAll()
                                   .Select(BookCopyToDtoProjection)
                                   .ToPagedListAsync(parameters, ct);

        _logger.LogDebug("Retrieved {Count} book copies", copies.Items.Count());

        return copies;
    }

    public async Task<Result<BookCopyDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving book copy with ID {BookCopyId}.", id);

        var dto = await _bookCopyRepository.GetAll()
                                            .Select(BookCopyToDtoProjection)
                                            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if(dto is null)
        {
            _logger.LogWarning("BookCopy {BookCopyId} was not found.", id);
            return Error.NotFound($"BookCopy '{id}' was not found.");
        }
        return dto;
    }

    public async Task<Result<BookCopyDto>> CreateAsync(CreateBookCopyRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new book copy with barcode {Barcode}", request.Barcode);

        bool barcodeExists = await _bookCopyRepository.BarcodeExistsAsync(request.Barcode, ct);
        if(barcodeExists)
        {
            _logger.LogWarning("Barcode {Barcode} already exists.", request.Barcode);
            return Error.Conflict($"A book copy with barcode '{request.Barcode}' already exists.");
        }

        var bookExists = await _bookRepository.ExistsAsync(request.BookId, ct);
        if(!bookExists)
        {
            _logger.LogWarning("Book {BookId} was not found when creating book copy.", request.BookId);
            return Error.NotFound($"Book '{request.BookId}' was not found.");
        }

        var copy = request.ToEntity();
        await _bookCopyRepository.AddAsync(copy, ct);

        _logger.LogInformation("Created BookCopy {BookCopyId}.", copy.Id);

        return copy.ToDto();
    }

    private static Expression<Func<BookCopy, BookCopyDto>> BookCopyToDtoProjection => bookCopy => new BookCopyDto
    {
        Id = bookCopy.Id,
        Barcode = bookCopy.Barcode,
        BookId = bookCopy.Id
    };

}
