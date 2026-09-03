using BookCatalog.API.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Dtos.Book;

public abstract record UpsertBookRequestDto
{
    public int AuthorId { get; init; }

    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [MaxLength(20)]
    public string Isbn { get; init; } = null!;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; init; }

    public BookGenre Genre { get; init; }

    public DateOnly PublicationDate { get; init; }
}
