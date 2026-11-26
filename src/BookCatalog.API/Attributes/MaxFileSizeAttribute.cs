using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Options;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Attributes;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly BlobCategory _blobCategory;
    public MaxFileSizeAttribute(BlobCategory blobCategory)
    {
        _blobCategory = blobCategory;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var blobOptions = validationContext.GetRequiredService<IOptionsMonitor<BlobOptions>>()
                                           .Get(_blobCategory.ToString());
        var maxSize = blobOptions.MaxSizeInBytes;

        var file = value as IFormFile;

        if(file is not null)
        {
            if(file.Length > maxSize)
                return new ValidationResult($"Maximum allowed file size is {maxSize / 1024 / 1024} MB.");
        }
        return ValidationResult.Success;
    }

}
