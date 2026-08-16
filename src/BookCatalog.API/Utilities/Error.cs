namespace BookCatalog.API.Utilities;

public class Error
{
    private readonly ICollection<ValidationError>? _validationErrors;

    public ICollection<ValidationError> ValidationErrors => Type is ErrorType.Validation ? _validationErrors! :
                                    throw new InvalidOperationException("Cannot access validation errors when the error type is not Validation");
    public ErrorType Type { get; }
    public string? ErrorTitle { get; }
    public string? ErrorDetail { get; }

    private Error(ErrorType errorType, string? errorTitle = null, string? errorDetail = null, ICollection<ValidationError>? validationErrors = null)
    {
        _validationErrors = validationErrors;
        Type = errorType;
        ErrorTitle = errorTitle;
        ErrorDetail = errorDetail;
    }
    public static Error NotFound(string? detail = null) =>
     new(ErrorType.NotFound, errorTitle: "Resource Not Found", errorDetail: detail);

    public static Error BadRequest(string? detail = null) =>
        new(ErrorType.BadRequest, errorTitle: "BadRequest", errorDetail: detail);

    public static Error Conflict(string? detail = null) =>
        new(ErrorType.Conflict, errorTitle: "Conflict", errorDetail: detail);

    public static Error Unauthorized(string? detail = null) =>
        new(ErrorType.Unauthorized, errorTitle: "Unauthorized", errorDetail: detail);

    public static Error Forbidden(string? detail = null) =>
        new(ErrorType.Forbidden, errorTitle: "Forbidden", errorDetail: detail);

    public static Error InternalServerError(string? detail = null) =>
        new(ErrorType.InternalError, errorTitle: "Internal Server Error", errorDetail: detail);

    public static Error TooManyRequests(string? detail = null) =>
        new(ErrorType.TooManyRequests, errorTitle: "Too Many Requests", errorDetail: detail);

    public static Error Validation(
      ICollection<ValidationError> validationErrors,
      string? detail = null) =>
      new(ErrorType.Validation, errorTitle: "Validation Errors", errorDetail: detail, validationErrors: validationErrors);

}
public record ValidationError(string PropertyName, string ErrorMessage);