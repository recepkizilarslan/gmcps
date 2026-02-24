using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gmcps.Tools.Configuration.Filters.ListFilters;

public sealed class ListFiltersInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
