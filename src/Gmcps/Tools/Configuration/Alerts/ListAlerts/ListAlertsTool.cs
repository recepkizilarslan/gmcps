
namespace Gmcps.Tools.Configuration.Alerts.ListAlerts;

public sealed class ListAlertsTool(
    IClient client)
    : ITool<ListAlertsInput, ListAlertsOutput>
{
    public async Task<Result<ListAlertsOutput>> ExecuteAsync(ListAlertsInput input, CancellationToken ct)
    {

        var response = await client.GetAlertsAsync(input.Limit, ct);

        return response.ToOutput<ListAlertsOutput>();
    }
}
