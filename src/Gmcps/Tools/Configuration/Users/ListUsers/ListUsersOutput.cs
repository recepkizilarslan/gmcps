namespace Gmcps.Tools.Configuration.Users.ListUsers;

public sealed record UserOutput(
    string Id,
    string Name);

public sealed record ListUsersOutput(
    IReadOnlyList<UserOutput> Users);

