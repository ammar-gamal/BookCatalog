using BookCatalog.API.Dtos.Common;
using BookCatalog.API.Dtos.User;
using BookCatalog.API.Entities;
using BookCatalog.API.ExtensionMethods;
using BookCatalog.API.ExtensionMethods.Mapping;
using BookCatalog.API.Repositories.Interfaces;
using BookCatalog.API.Services.Interfaces;
using BookCatalog.API.Utilities.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCatalog.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    public async Task<Result<PagedList<UserDto>>> GetAllAsync(PaginationQueryParameters parameters, CancellationToken ct = default)
    {
        var users = await _userRepository.GetAll()
                                         .Select(UserToDtoProjection)
                                         .OrderBy(e => e.Id)
                                         .ToPagedListAsync(parameters, ct);
        _logger.LogDebug("Retrieved {Count} users", users.Items.Count());

        return users;
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving user with ID {UserId}.", id);

        var dto = await _userRepository.GetAll()
                                       .Select(UserToDtoProjection)
                                       .FirstOrDefaultAsync(e => e.Id == id, ct);

        if(dto is null)
        {
            _logger.LogWarning("User {UserId} was not found.", id);
            return Error.NotFound($"User '{id}' was not found.");
        }
        return dto;
    }
    public async Task<Result<UserDto>> CreateAsync(CreateUserRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new user with email {Email}", request.Email);

        bool emailExists = await _userRepository.EmailExistsAsync(request.Email, ct);

        if(emailExists)
        {
            _logger.LogWarning("Email {Email} already exists.", request.Email);
            return Error.Conflict($"A user with email '{request.Email}' already exists.");
        }

        var user = request.ToEntity();
        await _userRepository.AddAsync(user, ct);

        _logger.LogInformation("Created User {UserId}.", user.Id);

        return user.ToDto();
    }
    private static Expression<Func<User, UserDto>> UserToDtoProjection => user => new UserDto
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name
    };

}
