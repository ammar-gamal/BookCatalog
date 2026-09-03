using BookCatalog.API.Dtos.Author;
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

public class AuthorService : IAuthorService
{
    private readonly IBaseRepository<Author> _authorRepository;
    private readonly ILogger<AuthorService> _logger;
    public AuthorService(IBaseRepository<Author> authorRepository, ILogger<AuthorService> logger)
    {
        _authorRepository = authorRepository;
        _logger = logger;
    }

    public async Task<Result<PagedList<AuthorDto>>> GetAllAsync(PaginationQueryParameters paramters, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all author.");

        var authors = await _authorRepository.GetAll()
                                   .Select(AuthorToDtoProjection)
                                   .OrderBy(e => e.Id)
                                   .ToPagedListAsync(paramters, ct);
        _logger.LogDebug("Retrieved {Count} authors", authors.Items.Count());

        return authors;
    }
    public async Task<Result<AuthorDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving author with ID {AuthorId}.", id);

        var dto = await _authorRepository.GetAll()
                                            .Select(AuthorToDtoProjection)
                                            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if(dto is null)
        {
            _logger.LogWarning("Author {AuthorId} was not found.", id);
            return Error.NotFound($"Author '{id}' was not found.");
        }
        return dto;
    }
    public async Task<Result<AuthorDto>> CreateAsync(CreateAuthorRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating new author {Name}", request.Name);

        var author = request.ToEntity();
        await _authorRepository.AddAsync(author, ct);

        _logger.LogInformation("Created Author {AuthorId}.", author.Id);

        return author.ToDto();
    }

    private static Expression<Func<Author, AuthorDto>> AuthorToDtoProjection => author => new AuthorDto
    {
        Id = author.Id,
        Name = author.Name,
        Biography = author.Biography
    };

}
