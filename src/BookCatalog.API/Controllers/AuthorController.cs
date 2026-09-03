using BookCatalog.API.Dtos.Author;
using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.API.Controllers;

[Route("api/authors")]
[ApiController]
public class AuthorController : AppController
{
    private readonly IAuthorService _authorService;

    public AuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<AuthorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQueryParameters paramters, CancellationToken ct)
    {
        var result = await _authorService.GetAllAsync(paramters, ct);

        return Ok(result.Data);
    }
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _authorService.GetByIdAsync(id, ct);
        if(!result.IsSuccess)
            return HandleError(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthorRequestDto request, CancellationToken ct)
    {
        var result = await _authorService.CreateAsync(request, ct);
        if(!result.IsSuccess)
            return HandleError(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }
}
