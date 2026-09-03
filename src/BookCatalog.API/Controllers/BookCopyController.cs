using BookCatalog.API.Dtos.BookCopy;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.API.Controllers;

[Route("api/book-copies")]
[ApiController]
public class BookCopyController : AppController
{
    private readonly IBookCopyService _bookCopyService;

    public BookCopyController(IBookCopyService bookCopyService)
    {
        _bookCopyService = bookCopyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedList<BookCopyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQueryParameters parameters, CancellationToken ct)
    {
        var result = await _bookCopyService.GetAllAsync(parameters, ct);

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookCopyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _bookCopyService.GetByIdAsync(id, ct);
        if(!result.IsSuccess)
            return HandleError(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookCopyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]

    public async Task<IActionResult> Create([FromBody] CreateBookCopyRequestDto request, CancellationToken ct)
    {
        var result = await _bookCopyService.CreateAsync(request, ct);
        if(!result.IsSuccess)
            return HandleError(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }
}
