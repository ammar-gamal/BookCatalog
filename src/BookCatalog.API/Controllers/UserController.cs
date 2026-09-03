using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Dtos.User;
using BookCatalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.API.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : AppController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(parameters, cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);
        if(!result.IsSuccess)
            return HandleError(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }
}
