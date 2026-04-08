namespace Amp.Facebook.Api.Models.Facebook;

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
