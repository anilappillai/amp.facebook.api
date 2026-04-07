using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

// ─── Graph API response shapes ────────────────────────────────────────────────

/// <summary>A Facebook page returned from /me/accounts.</summary>
public sealed class FacebookPageInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("tasks")]
    public List<string> Tasks { get; set; } = [];
}

/// <summary>Graph API paged list wrapper (data + paging cursor).</summary>
public sealed class FacebookPagedList<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];

    [JsonPropertyName("paging")]
    public FacebookPaging? Paging { get; set; }
}

public sealed class FacebookPaging
{
    [JsonPropertyName("cursors")]
    public FacebookCursors? Cursors { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

public sealed class FacebookCursors
{
    [JsonPropertyName("before")]
    public string? Before { get; set; }

    [JsonPropertyName("after")]
    public string? After { get; set; }
}

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

/// <summary>Returned after a successful delete operation.</summary>
public sealed class FacebookDeleteResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}

/// <summary>Facebook Graph API error payload.</summary>
public sealed class FacebookApiError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("error_subcode")]
    public int? ErrorSubcode { get; set; }

    [JsonPropertyName("fbtrace_id")]
    public string? FbTraceId { get; set; }
}

/// <summary>Top-level envelope for Graph API error responses.</summary>
public sealed class FacebookErrorEnvelope
{
    [JsonPropertyName("error")]
    public FacebookApiError? Error { get; set; }
}
