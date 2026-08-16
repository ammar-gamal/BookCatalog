namespace BookCatalog.API.Utilities;

public static class IsbnNormalizer
{
    public static string Normalize(string isbn)
    {
        return isbn.Trim().ToUpperInvariant();
    }
}
