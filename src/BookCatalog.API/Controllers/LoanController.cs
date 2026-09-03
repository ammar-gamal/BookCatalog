using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Dtos.Loan;
using BookCatalog.API.Services;
using BookCatalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.API.Controllers;

[Route("api/loans")]
[ApiController]
public class LoanController : AppController
{
    private readonly ILoanService _loanService;

    public LoanController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedList<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetAllAsync(parameters, cancellationToken);

        return Ok(result.Data);
    }

    [HttpGet("users/{userId:int}")]
    [ProducesResponseType(typeof(PagedList<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllForUser(int userId, [FromQuery] PaginationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetAllForUserAsync(userId, parameters, cancellationToken);

        return Ok(result.Data);
    }

    [HttpGet("book-copies/{bookCopyId:int}")]
    [ProducesResponseType(typeof(PagedList<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllForBookCopy(int bookCopyId, [FromQuery] PaginationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetAllForBookCopyAsync(bookCopyId, parameters, cancellationToken);

        return Ok(result.Data);
    }

    [HttpGet("books/{bookId:int}")]
    [ProducesResponseType(typeof(PagedList<LoanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllForBook(int bookId, [FromQuery] PaginationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetAllForBookAsync(bookId, parameters, cancellationToken);

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetByIdAsync(id, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BorrowBookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Borrow([FromBody] BorrowBookRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _loanService.BorrowBookAsync(request, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReturnBook(int id, CancellationToken cancellationToken)
    {
        var result = await _loanService.ReturnBookAsync(id, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);

        return NoContent();
    }
}