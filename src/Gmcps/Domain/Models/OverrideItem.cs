namespace Gmcps.Domain.Models;

public sealed record OverrideItem(
    string Id,
    string Text,
    double? NewSeverity,
    bool Active);
