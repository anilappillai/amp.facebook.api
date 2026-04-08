namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Request body for creating a text post on a Facebook page.</summary>
public sealed class PostToPageRequest
{
    /// <summary>The text content of the post.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional URL attached to the post (link preview).</summary>
    public string? Link { get; set; }

    /// <summary>Publish immediately (true) or create as a draft/scheduled post (false).</summary>
    public bool Published { get; set; } = true;

    /// <summary>
    /// Unix timestamp for scheduled publishing.
    /// Only used when <see cref="Published"/> is false.
    /// Must be at least 10 minutes and at most 6 months in the future.
    /// </summary>
    public long? ScheduledPublishTime { get; set; }
}
