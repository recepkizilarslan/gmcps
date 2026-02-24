
namespace Gmcps.Tools.Configuration.Schedules.ListSchedules;

public sealed class ListSchedulesTool(
    IClient client)
    : ITool<ListSchedulesInput, ListSchedulesOutput>
{
    public async Task<Result<ListSchedulesOutput>> ExecuteAsync(ListSchedulesInput input, CancellationToken ct)
    {

        var response = await client.GetSchedulesAsync(input.Limit, ct);

        return response.ToOutput<ListSchedulesOutput>();
    }
}
