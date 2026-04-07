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

/// <summary>Request body for uploading a photo to a Facebook page.</summary>
public sealed class UploadPhotoRequest
{
    /// <summary>
    /// Publicly accessible URL of the photo to upload.
    /// Provide either <see cref="Url"/> or <see cref="Base64"/>, not both.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Base-64 encoded image data (JPEG or PNG).
    /// Provide either <see cref="Url"/> or <see cref="Base64"/>, not both.
    /// </summary>
    public string? Base64 { get; set; }

    /// <summary>Optional caption for the photo.</summary>
    public string? Caption { get; set; }

    /// <summary>Publish immediately. Set false to add to the page's photo album without publishing.</summary>
    public bool Published { get; set; } = true;
}
