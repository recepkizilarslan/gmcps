namespace Gmcps.Tools.Configuration.Credentials.ListCredentials;

public sealed record CredentialOutput(
    string Id,
    string Name,
    string Type,
    string? Comment);

public sealed record ListCredentialsOutput(
    IReadOnlyList<CredentialOutput> Credentials);
