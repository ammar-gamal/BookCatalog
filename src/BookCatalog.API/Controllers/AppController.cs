using BookCatalog.API.ExtensionMethods;
using BookCatalog.API.Utilities;
using Microsoft.AspNetCore.Mvc;


namespace BookCatalog.API.Controllers;

public abstract class AppController : ControllerBase
{
    protected IActionResult HandleError(Result result)
    {
        var error = result.Error;
        var detail = error.ErrorDetail;
        var title = error.ErrorTitle;

        return error.Type switch
        {
            ErrorType.NotFound => Problem(detail: detail, statusCode: StatusCodes.Status404NotFound, title: title),
            ErrorType.Conflict => Problem(detail: detail, statusCode: StatusCodes.Status409Conflict, title: title),
            ErrorType.BadRequest => Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest, title: title),
            ErrorType.Unauthorized => Problem(detail: detail, statusCode: StatusCodes.Status401Unauthorized, title: title),
            ErrorType.Forbidden => Problem(detail: detail, statusCode: StatusCodes.Status403Forbidden, title: title),
            ErrorType.Validation => ValidationProblem(detail: detail, title: title, modelStateDictionary: ModelState.AddValidationErrors(result.Error.ValidationErrors)),
            ErrorType.TooManyRequests => Problem(detail: detail, statusCode: StatusCodes.Status429TooManyRequests, title: title),
            _ => Problem(detail: detail, statusCode: StatusCodes.Status500InternalServerError, title: title)
        };
    }

}
