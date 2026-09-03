using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Dtos.Loan;
using BookCatalog.API.Entities;
using BookCatalog.API.ExtensionMethods;
using BookCatalog.API.ExtensionMethods.Mapping;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services.Interfaces;
using BookCatalog.API.Utilities.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCatalog.API.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookCopyRepository _bookCopyRepository;
    private readonly IBaseRepository<User> _userRepository;
    private readonly TimeProvider _time;
    private readonly ILogger<LoanService> _logger;

    public LoanService(
        ILoanRepository loanRepository,
        IBookCopyRepository bookCopyRepository,
        IBaseRepository<User> userRepository,
        TimeProvider time,
        ILogger<LoanService> logger)
    {
        _loanRepository = loanRepository;
        _bookCopyRepository = bookCopyRepository;
        _userRepository = userRepository;
        _time = time;
        _logger = logger;

    }
    public async Task<Result<PagedList<LoanDto>>> GetAllAsync(PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all loans.");

        var query = _loanRepository.GetAll();
        var loans = await GetPagedLoansAsync(query, parameters, ct);

        _logger.LogDebug("Retrieved {Count} loans.", loans.Items.Count());

        return loans;
    }

    public async Task<Result<PagedList<LoanDto>>> GetAllForUserAsync(int userId, PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all loans for user {UserId}.", userId);

        var query = _loanRepository.GetAllForUser(userId);
        var loans = await GetPagedLoansAsync(query, parameters, ct);


        _logger.LogDebug("Retrieved {Count} loans for user {UserId}.", loans.Items.Count(), userId);

        return loans;
    }

    public async Task<Result<PagedList<LoanDto>>> GetAllForBookCopyAsync(int bookCopyId, PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all loans for book copy {BookCopyId}.", bookCopyId);

        var query = _loanRepository.GetAllForBookCopy(bookCopyId);
        var loans = await GetPagedLoansAsync(query, parameters, ct);


        _logger.LogDebug("Retrieved {Count} loans for book copy {BookCopyId}.", loans.Items.Count(), bookCopyId);

        return loans;
    }

    public async Task<Result<PagedList<LoanDto>>> GetAllForBookAsync(int bookId, PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all loans for book {BookId}.", bookId);

        var query = _loanRepository.GetAllForBook(bookId);
        var loans = await GetPagedLoansAsync(query, parameters, ct);


        _logger.LogDebug("Retrieved {Count} loans for book {BookId}.", loans.Items.Count(), bookId);

        return loans;
    }

    public async Task<Result<LoanDto>> GetByIdAsync(int loanId, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving loan with ID {LoanId}.", loanId);

        var dto = await _loanRepository.GetAll()
                                       .Select(LoanToDtoProjection)
                                       .FirstOrDefaultAsync(e => e.Id == loanId, ct);

        if(dto is null)
        {
            _logger.LogWarning("Loan {LoanId} was not found.", loanId);
            return Error.NotFound($"Loan '{loanId}' was not found.");
        }

        return dto;
    }

    public async Task<Result<BorrowBookResponseDto>> BorrowBookAsync(BorrowBookRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Borrowing book copy {BookCopyId} for user {UserId}.", request.BookCopyId, request.UserId);
        var utcNow = _time.GetUtcNow();
        if(request.DueDate <= utcNow)
        {
            _logger.LogWarning("Invalid due date {DueDate}. It must be after {utcNow}.",
               request.DueDate, utcNow);

            return Error.Validation([new(nameof(request.DueDate), $"Due date must be after ({utcNow})")]);
        }
        if(!await _bookCopyRepository.ExistsAsync(request.BookCopyId, ct))
        {
            _logger.LogWarning("Book copy {BookCopyId} was not found.", request.BookCopyId);
            return Error.NotFound($"Book copy '{request.BookCopyId}' was not found.");
        }
        if(!await _userRepository.ExistsAsync(request.UserId, ct))
        {
            _logger.LogWarning("User {UserId} was not found.", request.UserId);
            return Error.NotFound($"User '{request.UserId}' was not found.");
        }
        if(await _loanRepository.BookCopyHasActiveLoanAsync(request.BookCopyId, ct))
        {
            _logger.LogWarning("Book copy {BookCopyId} already has an active loan.", request.BookCopyId);
            return Error.Conflict($"Book copy '{request.BookCopyId}' already has an active loan.");
        }
        var loan = request.ToEntity(utcNow);
        await _loanRepository.AddAsync(loan, ct);
        _logger.LogInformation("Created loan {LoanId} for book copy {BookCopyId} and user {UserId}.",
            loan.Id, request.BookCopyId, request.UserId);

        return loan.ToBorrowBookResponseDto();
    }

    public async Task<Result> ReturnBookAsync(int loanId, CancellationToken ct = default)
    {
        _logger.LogInformation("Returning loan {LoanId}.", loanId);

        var loan = await _loanRepository.GetByIdAsync(loanId, ct);
        if(loan is null)
        {
            _logger.LogWarning("Loan {LoanId} was not found.", loanId);
            return Error.NotFound($"Loan '{loanId}' was not found.");
        }
        if(loan.ReturnedDate is not null)
        {
            _logger.LogWarning("Loan {LoanId} has already been returned.", loanId);
            return Error.Conflict($"Loan '{loanId}' has already been returned.");
        }
        loan.ReturnedDate = _time.GetUtcNow();
        await _loanRepository.UpdateAsync(loan, ct);
        _logger.LogInformation("Loan {LoanId} marked as returned.", loanId);

        return Result.Ok();
    }
    private static Task<PagedList<LoanDto>> GetPagedLoansAsync(
        IQueryable<Loan> query,
        PaginationQueryParameters parameters,
        CancellationToken ct)
    {

        return query.Select(LoanToDtoProjection)
                     .OrderBy(e => e.Id)
                     .ToPagedListAsync(parameters, ct);

    }
    private static Expression<Func<Loan, LoanDto>> LoanToDtoProjection => loan => new()
    {
        Id = loan.Id,
        BookTitle = loan.BookCopy.Book.Title,
        UserEmail = loan.User.Email,
        UserId = loan.UserId,
        BookCopyId = loan.BookCopyId,
        BookId = loan.BookCopy.BookId,
        DueDate = loan.DueDate,
        LoanDate = loan.LoanDate,
        ReturnedDate = loan.ReturnedDate
    };
}