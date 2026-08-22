using BookCatalog.API.Dtos;
using BookCatalog.API.Dtos.Book;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Services;
using BookCatalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : AppController
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedList<BookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] BookFilterQueryParameters paramters, CancellationToken cancellationToken)
    {
        var result = await _bookService.GetAllAsync(paramters, cancellationToken);

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _bookService.GetByIdAsync(id, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _bookService.CreateAsync(request, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateAsync(id, request, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _bookService.DeleteAsync(id, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);

        return NoContent();
    }

}
