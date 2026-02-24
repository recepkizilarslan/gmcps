namespace Gmcps.Configuration;

public sealed class ServerOptions
{
    public const string Section = "Server";

    public int RateLimitPerMinute { get; set; } = 60;

    public int MaxStdoutBytes { get; set; } = 4 * 1024 * 1024;

    public int MaxToolExecutionSeconds { get; set; } = 120;
}
