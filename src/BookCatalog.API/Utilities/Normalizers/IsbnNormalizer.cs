namespace BookCatalog.API.Utilities.Normalizers;

public static class IsbnNormalizer
{
    public static string Normalize(string isbn)
    {
        return isbn.Trim().ToUpperInvariant();
    }
}
