using Amp.Facebook.Api.Models.Facebook;
using Amp.Facebook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Amp.Facebook.Api.Controllers;

/// <summary>
/// Exposes Facebook Graph API operations for a single page.
///
/// Access tokens are supplied by the caller per-request via the
/// X-User-Access-Token  (user token) or X-Page-Access-Token (page token) headers.
/// They are NEVER stored server-side.
/// </summary>
[ApiController]
[Route("api/facebook")]
[Produces("application/json")]
public class FacebookController(
    IFacebookService facebook,
    ILogger<FacebookController> logger) : ControllerBase
{
    // ── Pages ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all Facebook pages managed by the authenticated user.
    /// </summary>
    /// <remarks>
    /// Requires a valid <b>user access token</b> in the <c>X-User-Access-Token</c> header.
    /// The token must have the <c>pages_show_list</c> permission.
    /// </remarks>
    [HttpGet("pages")]
    [ProducesResponseType(typeof(List<FacebookPageInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetPagesAsync(
        [FromHeader(Name = "X-User-Access-Token")] string userAccessToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userAccessToken))
            return BadRequest("X-User-Access-Token header is required.");

        var pages = await facebook.GetUserPagesAsync(userAccessToken, ct);
        return Ok(pages);
    }

    // ── Posts ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Publishes a text post (with an optional link) to a Facebook page.
    /// </summary>
    /// <remarks>
    /// Requires a valid <b>page access token</b> in the <c>X-Page-Access-Token</c> header.
    /// The token must have the <c>pages_manage_posts</c> permission.
    /// </remarks>
    /// <param name="pageId">The Facebook page ID.</param>
    [HttpPost("pages/{pageId}/posts")]
    [ProducesResponseType(typeof(FacebookCreateResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreatePostAsync(
        string pageId,
        [FromHeader(Name = "X-Page-Access-Token")] string pageAccessToken,
        [FromBody] PostToPageRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pageAccessToken))
            return BadRequest("X-Page-Access-Token header is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var result = await facebook.CreatePostAsync(pageId, pageAccessToken, request, ct);
        logger.LogInformation("Created post {PostId} on page {PageId}", result.Id, pageId);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Deletes a post from a Facebook page.
    /// </summary>
    /// <remarks>
    /// Requires a valid <b>page access token</b> in the <c>X-Page-Access-Token</c> header.
    /// The token must have the <c>pages_manage_posts</c> permission.
    /// </remarks>
    /// <param name="postId">The post ID to delete (format: <c>{page-id}_{post-id}</c>).</param>
    [HttpDelete("posts/{postId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> DeletePostAsync(
        string postId,
        [FromHeader(Name = "X-Page-Access-Token")] string pageAccessToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pageAccessToken))
            return BadRequest("X-Page-Access-Token header is required.");

        var success = await facebook.DeletePostAsync(postId, pageAccessToken, ct);
        if (!success)
            return StatusCode(StatusCodes.Status502BadGateway, "Facebook did not confirm deletion.");

        logger.LogInformation("Deleted post {PostId}", postId);
        return NoContent();
    }

    // ── Photos ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Uploads a photo to a Facebook page.
    /// Supply either a public <c>Url</c> or base-64 encoded image data in <c>Base64</c>.
    /// </summary>
    /// <remarks>
    /// Requires a valid <b>page access token</b> in the <c>X-Page-Access-Token</c> header.
    /// The token must have the <c>pages_manage_posts</c> permission.
    /// </remarks>
    /// <param name="pageId">The Facebook page ID.</param>
    [HttpPost("pages/{pageId}/photos")]
    [ProducesResponseType(typeof(FacebookCreateResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> UploadPhotoAsync(
        string pageId,
        [FromHeader(Name = "X-Page-Access-Token")] string pageAccessToken,
        [FromBody] UploadPhotoRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pageAccessToken))
            return BadRequest("X-Page-Access-Token header is required.");

        if (string.IsNullOrWhiteSpace(request.Url) && string.IsNullOrWhiteSpace(request.Base64))
            return BadRequest("Either Url or Base64 must be provided.");

        var result = await facebook.UploadPhotoAsync(pageId, pageAccessToken, request, ct);
        logger.LogInformation("Uploaded photo {PhotoId} to page {PageId}", result.Id, pageId);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Deletes a photo from a Facebook page.
    /// </summary>
    /// <remarks>
    /// Requires a valid <b>page access token</b> in the <c>X-Page-Access-Token</c> header.
    /// The token must have the <c>pages_manage_posts</c> permission.
    /// </remarks>
    /// <param name="photoId">The photo ID to delete.</param>
    [HttpDelete("photos/{photoId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> DeletePhotoAsync(
        string photoId,
        [FromHeader(Name = "X-Page-Access-Token")] string pageAccessToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pageAccessToken))
            return BadRequest("X-Page-Access-Token header is required.");

        var success = await facebook.DeletePhotoAsync(photoId, pageAccessToken, ct);
        if (!success)
            return StatusCode(StatusCodes.Status502BadGateway, "Facebook did not confirm deletion.");

        logger.LogInformation("Deleted photo {PhotoId}", photoId);
        return NoContent();
    }
}
