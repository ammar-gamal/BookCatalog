using BookCatalog.API.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Dtos.Abstractions;

public abstract record UpsertBookRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Author { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [Required]
    [MaxLength(20)]
    public string Isbn { get; init; } = null!;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; init; }

    public BookGenre Genre { get; init; }

    public DateOnly PublicationYear { get; init; }
}
