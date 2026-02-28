namespace Gmcps.Configuration;

public sealed class GvmOptions
{
    public const string Section = "Gvm";

    // Unix socket settings
    public string SocketPath { get; set; } = "/run/gvmd/gvmd.sock";

    // Optional default port list used by gvm_create_target when input.portListId is omitted.
    public string DefaultPortListId { get; set; } = string.Empty;

    // Common settings
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}
