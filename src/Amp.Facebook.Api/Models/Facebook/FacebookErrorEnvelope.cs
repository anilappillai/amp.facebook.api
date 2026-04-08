using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Top-level envelope for Graph API error responses.</summary>
public sealed class FacebookErrorEnvelope
{
    [JsonPropertyName("error")]
    public FacebookApiError? Error { get; set; }
}
