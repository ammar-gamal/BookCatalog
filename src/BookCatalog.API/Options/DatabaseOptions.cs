namespace BookCatalog.API.Options;

public class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [ConfigurationKeyName("Database")]
    public string ConnectionString { get; set; } = string.Empty;
}
