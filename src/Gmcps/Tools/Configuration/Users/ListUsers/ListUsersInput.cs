
namespace Gmcps.Tools.Configuration.Users.ListUsers;

public sealed class ListUsersInput
{
    [JsonPropertyName("limit")]
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;
}

