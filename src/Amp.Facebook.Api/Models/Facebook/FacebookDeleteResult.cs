using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Returned after a successful delete operation.</summary>
public sealed class FacebookDeleteResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
