using System.ComponentModel.DataAnnotations;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Request body for uploading multiple photos to a Facebook page in one call.</summary>
public sealed class UploadPhotosBatchRequest
{
    /// <summary>
    /// List of photos to upload. Maximum 10 items per request.
    /// Each item must supply either <see cref="UploadPhotoRequest.Url"/> or
    /// <see cref="UploadPhotoRequest.Base64"/>.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one photo is required.")]
    [MaxLength(10, ErrorMessage = "A maximum of 10 photos can be uploaded per request.")]
    public List<UploadPhotoRequest> Photos { get; set; } = [];
}
