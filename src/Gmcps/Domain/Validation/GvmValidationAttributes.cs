namespace Gmcps.Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class GvmIdAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string id)
        {
            return new ValidationResult("Invalid identifier format.");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Success;
        }

        if (id.Length > 128 || !IdRegex().IsMatch(id))
        {
            return new ValidationResult("Invalid identifier format.");
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex("^[A-Za-z0-9-]+$", RegexOptions.Compiled)]
    private static partial Regex IdRegex();
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class GvmOidAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string oid)
        {
            return new ValidationResult("Invalid OID format.");
        }

        if (string.IsNullOrWhiteSpace(oid))
        {
            return ValidationResult.Success;
        }

        if (oid.Length > 128 || !OidRegex().IsMatch(oid))
        {
            return new ValidationResult("Invalid OID format.");
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex("^\\d+(?:\\.\\d+)+$", RegexOptions.Compiled)]
    private static partial Regex OidRegex();
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class GvmIdCollectionAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not IEnumerable items)
        {
            return new ValidationResult("Invalid identifier format.");
        }

        foreach (var item in items)
        {
            if (item is not string id || string.IsNullOrWhiteSpace(id) || id.Length > 128 || !IdRegex().IsMatch(id))
            {
                return new ValidationResult("Invalid identifier format.");
            }
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex("^[A-Za-z0-9-]+$", RegexOptions.Compiled)]
    private static partial Regex IdRegex();
}
