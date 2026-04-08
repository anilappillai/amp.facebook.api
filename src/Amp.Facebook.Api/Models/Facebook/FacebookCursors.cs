using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

public sealed class FacebookCursors
{
    [JsonPropertyName("before")]
    public string? Before { get; set; }

    [JsonPropertyName("after")]
    public string? After { get; set; }
}
