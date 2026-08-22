namespace BookCatalog.API.Utilities.Results;

public enum ErrorType
{
    NotFound,
    Validation,
    Conflict,
    BadRequest,
    Unauthorized,
    Forbidden,
    TooManyRequests,
    InternalError
}
