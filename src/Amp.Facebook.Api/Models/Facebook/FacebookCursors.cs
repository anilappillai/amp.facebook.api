using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Cursor values for navigating paginated Graph API results.</summary>
public sealed class FacebookCursors
{
    /// <summary>Cursor pointing to the start of the current result set.</summary>
    [JsonPropertyName("before")]
    public string? Before { get; set; }

    /// <summary>Cursor pointing to the end of the current result set.</summary>
    [JsonPropertyName("after")]
    public string? After { get; set; }
}
