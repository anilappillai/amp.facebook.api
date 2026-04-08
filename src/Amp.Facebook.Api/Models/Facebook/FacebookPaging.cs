using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

public sealed class FacebookPaging
{
    [JsonPropertyName("cursors")]
    public FacebookCursors? Cursors { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
