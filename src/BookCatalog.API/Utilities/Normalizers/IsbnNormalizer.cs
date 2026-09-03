namespace BookCatalog.API.Utilities.Normalizers;

public static class IsbnNormalizer
{
    public static string Normalize(string isbn)
    {
        return isbn
             .Replace("-", "")
             .Replace(" ", "")
             .Trim()
             .ToUpperInvariant();
    }
}
