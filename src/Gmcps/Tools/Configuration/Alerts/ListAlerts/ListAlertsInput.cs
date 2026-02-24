
namespace Gmcps.Tools.Configuration.Alerts.ListAlerts;

public sealed class ListAlertsInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}
