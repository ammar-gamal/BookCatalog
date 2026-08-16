namespace BookCatalog.API.Utilities;

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
