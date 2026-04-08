using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Returned after successfully creating a post or photo.</summary>
public sealed class FacebookCreateResult
{
    /// <summary>The ID of the created post / photo. Format: {page-id}_{object-id}.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Post ID returned specifically from the /photos endpoint.</summary>
    [JsonPropertyName("post_id")]
    public string? PostId { get; set; }
}
