namespace Gmcps.Tools.Configuration.Schedules.ListSchedules;

public sealed record ScheduleOutput(
    string Id,
    string Name,
    string? Timezone,
    string? Icalendar);

public sealed record ListSchedulesOutput(
    IReadOnlyList<ScheduleOutput> Schedules);
