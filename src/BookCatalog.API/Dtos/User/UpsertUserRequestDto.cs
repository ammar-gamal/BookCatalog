using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Dtos.User;

public abstract record UpsertUserRequestDto
{
    [MaxLength(100)]
    public string Name { get; init; } = null!;

    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; init; } = null!;
}
