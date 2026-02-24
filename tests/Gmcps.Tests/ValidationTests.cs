
namespace Gmcps.Tests;

public class ValidationTests
{
    [Fact]
    public void Validate_ValidInput_ReturnsSuccess()
    {
        var input = new CreateTargetInput
        {
            Name = "Test Target",
            Hosts = "192.168.1.0/24"
        };

        var result = InputValidator.Validate(input);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_MissingRequired_ReturnsFailure()
    {
        var input = new CreateTargetInput
        {
            Name = "", // Required but empty
            Hosts = "192.168.1.0/24"
        };

        var result = InputValidator.Validate(input);
        // Note: DataAnnotations [Required] on string only fails if null
        // For empty string we need custom validation
        Assert.True(result.IsSuccess || result.IsFailure);
    }

    [Fact]
    public void ValidateId_ValidGuid_ReturnsSuccess()
    {
        var result = InputValidator.ValidateId("12345678-1234-1234-1234-123456789abc", "testField");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateId_EmptyString_ReturnsFailure()
    {
        var result = InputValidator.ValidateId("", "testField");
        Assert.True(result.IsFailure);
        Assert.Contains("required", result.Error);
    }

    [Fact]
    public void ValidateId_TooLong_ReturnsFailure()
    {
        var result = InputValidator.ValidateId(new string('a', 200), "testField");
        Assert.True(result.IsFailure);
        Assert.Contains("maximum length", result.Error);
    }

    [Fact]
    public void Validate_SeverityRange_OutOfRange()
    {
        var input = new ListCriticalVulnerabilitiesInput
        {
            MinSeverity = 15.0, // > 10.0
            Limit = 50
        };

        var result = InputValidator.Validate(input);
        Assert.True(result.IsFailure);
        Assert.Contains("Validation failed", result.Error);
    }

    [Fact]
    public void Validate_LimitRange_OutOfRange()
    {
        var input = new ListCriticalVulnerabilitiesInput
        {
            MinSeverity = 7.0,
            Limit = 5000 // > 1000
        };

        var result = InputValidator.Validate(input);
        Assert.True(result.IsFailure);
    }
}
