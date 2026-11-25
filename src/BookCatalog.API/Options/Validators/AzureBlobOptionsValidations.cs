using Microsoft.Extensions.Options;

namespace BookCatalog.API.Options.Validators;

public class AzureBlobOptionsValidations : IValidateOptions<BlobStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, BlobStorageOptions options)
    {
        if(options is null)
            return ValidateOptionsResult.Fail("AzureBlobOptions is missing.");

        if(string.IsNullOrWhiteSpace(options.Endpoint))
            return ValidateOptionsResult.Fail("Endpoint is missing or empty.");

        return ValidateOptionsResult.Success;

    }
}
