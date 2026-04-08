using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>A Facebook page returned from /me/accounts.</summary>
public sealed class FacebookPageInfo
{
    /// <summary>The unique Facebook page ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The display name of the Facebook page.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Page-scoped access token for managing this page.</summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>The category of the Facebook page (e.g. "Brand", "Local Business").</summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>List of tasks the current user can perform on this page.</summary>
    [JsonPropertyName("tasks")]
    public List<string> Tasks { get; set; } = [];
}
