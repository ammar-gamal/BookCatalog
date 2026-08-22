using BookCatalog.API.Utilities.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookCatalog.API.ExtensionMethods;

public static class ModelStateExtensions
{
    public static ModelStateDictionary AddValidationErrors(this ModelStateDictionary modelState, ICollection<ValidationError> validationErrors)
    {
        var dictionary = validationErrors.GroupBy(e => e.PropertyName)
                                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage));
        foreach(var kv in dictionary)
        {
            foreach(var value in kv.Value)
            {
                modelState.AddModelError(kv.Key, value);
            }
        }
        return modelState;

    }
}
