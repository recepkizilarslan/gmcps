using System.ComponentModel.DataAnnotations;
using Gmcps.Domain;

namespace Gmcps.Infrastructure.Security;

public static class InputValidator
{
    public static Result<T> Validate<T>(T input) where T : class
    {
        var context = new ValidationContext(input);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(input, context, results, validateAllProperties: true))
        {
            var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            return Result<T>.Failure($"Validation failed: {errors}");
        }

        return Result<T>.Success(input);
    }
    
    public static Result<string> ValidateId(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<string>.Failure($"{fieldName} is required");
        }

        if (value.Length > 128)
        {
            return Result<string>.Failure($"{fieldName} exceeds maximum length");
        }
        
        if (!IsValidGuid(value) && !value.All(c => char.IsLetterOrDigit(c) || c == '-'))
        {
            return Result<string>.Failure($"{fieldName} contains invalid characters");
        }

        return Result<string>.Success(value);
    }
    
    private static bool IsValidGuid(string value) =>
        Guid.TryParse(value, out _);
}
