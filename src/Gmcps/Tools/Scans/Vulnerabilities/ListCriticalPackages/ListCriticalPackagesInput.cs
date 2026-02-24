
namespace Gmcps.Domain.Scans.Vulnerabilities.Inputs;

public sealed class ListCriticalPackagesInput
{
    [JsonPropertyName("os")]
    public OsFilter Os { get; set; } = OsFilter.Any;

    [JsonPropertyName("minSeverity")]
    [Range(0.0, 10.0)]
    public double MinSeverity { get; set; } = 7.0;

    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
