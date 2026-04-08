using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Facebook Graph API error payload.</summary>
public sealed class FacebookApiError
{
    /// <summary>Human-readable error message from the Graph API.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Error type classification (e.g. "OAuthException", "GraphMethodException").</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Numeric Facebook error code.</summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>Optional subcode providing further detail on the error.</summary>
    [JsonPropertyName("error_subcode")]
    public int? ErrorSubcode { get; set; }

    /// <summary>Facebook trace ID for support and debugging.</summary>
    [JsonPropertyName("fbtrace_id")]
    public string? FbTraceId { get; set; }
}
