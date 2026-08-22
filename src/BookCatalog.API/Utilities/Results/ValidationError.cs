namespace BookCatalog.API.Utilities.Results;

public record ValidationError(string PropertyName, string ErrorMessage);
