using BookCatalog.API.Entities;
using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Persistence;
using BookCatalog.API.Utilities.Normalizers;

namespace BookCatalog.IntegrationTests.Infrastructure;

public class TestDatabaseSeeder
{
    private readonly AppDbContext _db;

    public TestDatabaseSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Author> SeedAuthorAsync(
        string? name = null,
        string? biography = null,
        CancellationToken ct = default)
    {
        var author = new Author
        {
            Name = name ?? $"Author_{Guid.NewGuid():N}"[..20],
            Biography = biography ?? "Test Biography"
        };

        _db.Authors.Add(author);
        await _db.SaveChangesAsync(ct);
        return author;
    }

    public async Task<Book> SeedBookAsync(
        int authorId,
        string? title = null,
        string? isbn = null,
        decimal price = 29.99m,
        BookGenre genre = BookGenre.Fiction,
        DateOnly? publicationDate = null,
        string? description = null,
        CancellationToken ct = default)
    {
        isbn ??= $"978{Guid.NewGuid():N}"[..13];
        var normalizedIsbn = IsbnNormalizer.Normalize(isbn);

        var book = new Book
        {
            AuthorId = authorId,
            Title = title ?? $"Book_{Guid.NewGuid():N}"[..25],
            Isbn = isbn,
            NormalizedIsbn = normalizedIsbn,
            Price = price,
            Genre = genre,
            PublicationDate = publicationDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            Description = description ?? "Test Description"
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync(ct);
        return book;
    }

    public async Task<User> SeedUserAsync(
        string? name = null,
        string? email = null,
        CancellationToken ct = default)
    {
        var user = new User
        {
            Name = name ?? $"User_{Guid.NewGuid():N}"[..15],
            Email = email ?? $"user_{Guid.NewGuid():N}@test.com"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<BookCopy> SeedBookCopyAsync(
        int bookId,
        string? barcode = null,
        CancellationToken ct = default)
    {
        var copy = new BookCopy
        {
            BookId = bookId,
            Barcode = barcode ?? $"BC_{Guid.NewGuid():N}"[..15]
        };

        _db.BookCopies.Add(copy);
        await _db.SaveChangesAsync(ct);
        return copy;
    }

    public async Task<Loan> SeedActiveLoanAsync(
        int bookCopyId,
        int userId,
        DateTimeOffset? dueDate = null,
        CancellationToken ct = default)
    {
        var loan = new Loan
        {
            BookCopyId = bookCopyId,
            UserId = userId,
            LoanDate = DateTimeOffset.UtcNow.AddDays(-1),
            DueDate = dueDate ?? DateTimeOffset.UtcNow.AddDays(7),
            ReturnedDate = null
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync(ct);
        return loan;
    }

    public async Task<(Author Author, Book Book, BookCopy Copy, User User, Loan Loan)> SeedCompleteActiveLoanForBookAsync(
        CancellationToken ct = default)
    {
        var author = await SeedAuthorAsync(ct: ct);
        var book = await SeedBookAsync(author.Id, ct: ct);
        var copy = await SeedBookCopyAsync(book.Id, ct: ct);
        var user = await SeedUserAsync(ct: ct);
        var loan = await SeedActiveLoanAsync(copy.Id, user.Id, ct: ct);

        return (author, book, copy, user, loan);
    }
}
