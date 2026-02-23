namespace Gmcps.Configuration;

public sealed class GvmOptions
{
    public const string Section = "Gvm";

    // Unix socket settings
    public string SocketPath { get; set; } = "/run/gvmd/gvmd.sock";

    // Common settings
    public string Username { get; set; } = "admin";
    
    public string Password { get; set; } = "admin";
   
    public int TimeoutSeconds { get; set; } = 30;
}
