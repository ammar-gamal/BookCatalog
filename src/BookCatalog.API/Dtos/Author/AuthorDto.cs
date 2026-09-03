namespace BookCatalog.API.Dtos.Author;

public record AuthorDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Biography { get; init; }
}
