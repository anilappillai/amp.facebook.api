namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Result for a single photo within a batch upload operation.</summary>
public sealed class UploadPhotoBatchItemResult
{
    /// <summary>Zero-based index of the photo in the original request list.</summary>
    public int Index { get; init; }

    /// <summary>True when the photo was uploaded successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Facebook photo ID assigned on success; null on failure.</summary>
    public string? PhotoId { get; init; }

    /// <summary>Error message on failure; null on success.</summary>
    public string? Error { get; init; }
}
