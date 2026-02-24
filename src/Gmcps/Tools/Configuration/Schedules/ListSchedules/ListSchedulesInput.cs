
namespace Gmcps.Tools.Configuration.Schedules.ListSchedules;

public sealed class ListSchedulesInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
