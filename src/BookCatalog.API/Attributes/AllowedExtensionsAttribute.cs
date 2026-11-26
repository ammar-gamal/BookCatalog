using BookCatalog.API.Entities.Enums;
using BookCatalog.API.Options;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.API.Attributes;

public sealed class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly BlobCategory _blobCategory;

    public AllowedExtensionsAttribute(BlobCategory blobCategory)
    {
        _blobCategory = blobCategory;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(value is null)
            return ValidationResult.Success;

        if(value is not IFormFile file)
            throw new InvalidOperationException(
                $"{nameof(AllowedExtensionsAttribute)} can only be applied to {nameof(IFormFile)} members.");

        var blobOptions = validationContext.GetRequiredService<IOptionsMonitor<BlobOptions>>()
                               .Get(_blobCategory.ToString());

        var extension = Path.GetExtension(file.FileName);
        var allowedExtensions = blobOptions.AllowedExtensions;

        if(string.IsNullOrEmpty(extension) ||
            !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return new ValidationResult($"File extension {extension} is not allowed");

        return ValidationResult.Success;
    }

}
