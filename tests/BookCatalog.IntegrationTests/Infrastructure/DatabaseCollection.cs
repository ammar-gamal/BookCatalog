namespace BookCatalog.IntegrationTests.Infrastructure;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<BookCatalogWebApplicationFactory>
{ }
