using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Dtos.Author;

public abstract record UpsertAuthorRequestDto
{
    [MaxLength(250)]
    public string Name { get; init; } = null!;

    [MaxLength(1000)]
    public string? Biography { get; set; }
}
