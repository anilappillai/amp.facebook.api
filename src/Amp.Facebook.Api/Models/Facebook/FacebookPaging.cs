using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Paging metadata returned with Graph API list responses.</summary>
public sealed class FacebookPaging
{
    /// <summary>Before/after cursors for cursor-based pagination.</summary>
    [JsonPropertyName("cursors")]
    public FacebookCursors? Cursors { get; set; }

    /// <summary>URL to fetch the next page of results, if available.</summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
