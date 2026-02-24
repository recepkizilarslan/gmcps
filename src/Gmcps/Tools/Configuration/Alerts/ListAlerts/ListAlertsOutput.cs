namespace Gmcps.Tools.Configuration.Alerts.ListAlerts;

public sealed record AlertOutput(
    string Id,
    string Name,
    string Event,
    string? Comment);

public sealed record ListAlertsOutput(
    IReadOnlyList<AlertOutput> Alerts);
