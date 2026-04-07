using Amp.Facebook.Api.Models.Facebook;

namespace Amp.Facebook.Api.Services;

/// <summary>
/// Abstraction over the Facebook Graph API.
/// All operations require a valid access token supplied per-call —
/// no tokens are stored in this service.
/// </summary>
public interface IFacebookService
{
    /// <summary>
    /// Returns all Facebook pages managed by the user identified by
    /// <paramref name="userAccessToken"/>.
    /// Calls: GET /me/accounts
    /// </summary>
    Task<List<FacebookPageInfo>> GetUserPagesAsync(
        string userAccessToken,
        CancellationToken ct = default);

    /// <summary>
    /// Publishes a text post (and optional link) on a Facebook page.
    /// Calls: POST /{pageId}/feed
    /// </summary>
    Task<FacebookCreateResult> CreatePostAsync(
        string pageId,
        string pageAccessToken,
        PostToPageRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a post from a Facebook page.
    /// Calls: DELETE /{postId}
    /// </summary>
    Task<bool> DeletePostAsync(
        string postId,
        string pageAccessToken,
        CancellationToken ct = default);

    /// <summary>
    /// Uploads a photo to a Facebook page (by URL or base-64 data).
    /// Calls: POST /{pageId}/photos
    /// </summary>
    Task<FacebookCreateResult> UploadPhotoAsync(
        string pageId,
        string pageAccessToken,
        UploadPhotoRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a photo from a Facebook page.
    /// Calls: DELETE /{photoId}
    /// </summary>
    Task<bool> DeletePhotoAsync(
        string photoId,
        string pageAccessToken,
        CancellationToken ct = default);
}
