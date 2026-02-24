namespace Gmcps.Domain.Models;

public sealed record GvmSchedule(
    string Id,
    string Name,
    string? Timezone,
    string? Icalendar);
