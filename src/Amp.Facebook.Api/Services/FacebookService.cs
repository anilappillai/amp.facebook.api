using Amp.Facebook.Api.Infrastructure;
using Amp.Facebook.Api.Models.Facebook;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Amp.Facebook.Api.Services;

/// <summary>
/// Calls the Facebook Graph API v25.0 using a named <see cref="HttpClient"/>
/// registered in DI as "FacebookGraph".
///
/// Tokens are NEVER cached or stored here — callers supply them per request.
/// All HTTP errors from Facebook are converted to <see cref="FacebookApiException"/>.
/// </summary>
public sealed class FacebookService(
    IHttpClientFactory httpClientFactory,
    ILogger<FacebookService> logger) : IFacebookService
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<FacebookPageInfo>> GetUserPagesAsync(
        string userAccessToken, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching Facebook pages for user");

        var client = CreateClient(userAccessToken);
        var response = await client.GetAsync(FacebookApiConstants.EndpointMeAccounts, ct);
        var body = await EnsureSuccessAsync(response, "GET /me/accounts", ct);

        var paged = JsonSerializer.Deserialize<FacebookPagedList<FacebookPageInfo>>(body, _json);
        return paged?.Data ?? [];
    }

    /// <inheritdoc/>
    public async Task<FacebookCreateResult> CreatePostAsync(
        string pageId, string pageAccessToken, PostToPageRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Creating post on Facebook page {PageId}", pageId);

        var form = new Dictionary<string, string>
        {
            [FacebookApiConstants.FormMessage] = request.Message
        };

        if (!string.IsNullOrWhiteSpace(request.Link))
            form[FacebookApiConstants.FormLink] = request.Link;

        if (!request.Published)
        {
            form[FacebookApiConstants.FormPublished] = "false";
            if (request.ScheduledPublishTime.HasValue)
                form[FacebookApiConstants.FormScheduledPublishTime] = request.ScheduledPublishTime.Value.ToString();
        }

        var client = CreateClient(pageAccessToken);
        var response = await client.PostAsync($"{pageId}/feed", new FormUrlEncodedContent(form), ct);
        var body = await EnsureSuccessAsync(response, $"POST /{pageId}/feed", ct);

        return JsonSerializer.Deserialize<FacebookCreateResult>(body, _json)
               ?? throw new FacebookApiException(FacebookApiConstants.ErrorEmptyPostResponse);
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePostAsync(
        string postId, string pageAccessToken, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting Facebook post {PostId}", postId);

        var client = CreateClient(pageAccessToken);
        var response = await client.DeleteAsync(postId, ct);
        var body = await EnsureSuccessAsync(response, $"DELETE /{postId}", ct);

        var result = JsonSerializer.Deserialize<FacebookDeleteResult>(body, _json);
        return result?.Success ?? false;
    }

    /// <inheritdoc/>
    public async Task<FacebookCreateResult> UploadPhotoAsync(
        string pageId, string pageAccessToken, UploadPhotoRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Uploading photo to Facebook page {PageId}", pageId);

        var client = CreateClient(pageAccessToken);
        HttpResponseMessage response;

        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            // Upload by URL — Graph API fetches the image itself
            var form = new Dictionary<string, string> { [FacebookApiConstants.FormUrl] = request.Url };
            if (!string.IsNullOrWhiteSpace(request.Caption))
                form[FacebookApiConstants.FormCaption] = request.Caption;
            form[FacebookApiConstants.FormPublished] = request.Published ? "true" : "false";

            response = await client.PostAsync($"{pageId}/photos", new FormUrlEncodedContent(form), ct);
        }
        else if (!string.IsNullOrWhiteSpace(request.Base64))
        {
            // Upload raw image bytes
            var bytes = Convert.FromBase64String(request.Base64);
            var multipart = new MultipartFormDataContent
            {
                { new ByteArrayContent(bytes), FacebookApiConstants.FormSource, FacebookApiConstants.FormPhotoFileName }
            };
            if (!string.IsNullOrWhiteSpace(request.Caption))
                multipart.Add(new StringContent(request.Caption), FacebookApiConstants.FormCaption);
            multipart.Add(new StringContent(request.Published ? "true" : "false"), FacebookApiConstants.FormPublished);

            response = await client.PostAsync($"{pageId}/photos", multipart, ct);
        }
        else
        {
            throw new ArgumentException(FacebookApiConstants.ErrorUrlOrBase64RequiredService);
        }

        var body = await EnsureSuccessAsync(response, $"POST /{pageId}/photos", ct);
        return JsonSerializer.Deserialize<FacebookCreateResult>(body, _json)
               ?? throw new FacebookApiException(FacebookApiConstants.ErrorEmptyPhotoResponse);
    }

    /// <inheritdoc/>
    public async Task<List<UploadPhotoBatchItemResult>> UploadPhotosAsync(
        string pageId, string pageAccessToken, IReadOnlyList<UploadPhotoRequest> requests, CancellationToken ct = default)
    {
        logger.LogInformation("Uploading {Count} photos to Facebook page {PageId}", requests.Count, pageId);

        var tasks = requests.Select((request, index) =>
            UploadSingleAsync(pageId, pageAccessToken, request, index, ct));

        var results = await Task.WhenAll(tasks);
        return [.. results];
    }

    private async Task<UploadPhotoBatchItemResult> UploadSingleAsync(
        string pageId, string pageAccessToken, UploadPhotoRequest request, int index, CancellationToken ct)
    {
        try
        {
            var result = await UploadPhotoAsync(pageId, pageAccessToken, request, ct);
            return new UploadPhotoBatchItemResult { Index = index, Success = true, PhotoId = result.Id };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Batch photo upload failed for index {Index} on page {PageId}", index, pageId);
            return new UploadPhotoBatchItemResult { Index = index, Success = false, Error = ex.Message };
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePhotoAsync(
        string photoId, string pageAccessToken, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting Facebook photo {PhotoId}", photoId);

        var client = CreateClient(pageAccessToken);
        var response = await client.DeleteAsync(photoId, ct);
        var body = await EnsureSuccessAsync(response, $"DELETE /{photoId}", ct);


        var result = JsonSerializer.Deserialize<FacebookDeleteResult>(body, _json);
        return result?.Success ?? false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private HttpClient CreateClient(string accessToken)
    {
        var client = httpClientFactory.CreateClient(FacebookApiConstants.HttpClientName);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(FacebookApiConstants.AuthScheme, accessToken);
        return client;
    }

    private async Task<string> EnsureSuccessAsync(
        HttpResponseMessage response, string operation, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
            return body;

        // Try to parse a Facebook error envelope
        FacebookApiError? error = null;
        try
        {
            var envelope = JsonSerializer.Deserialize<FacebookErrorEnvelope>(body, _json);
            error = envelope?.Error;
        }
        catch { /* ignore parse failure */ }

        var message = error is not null
            ? $"Facebook API error (code {error.Code}): {error.Message}"
            : $"Facebook API {operation} failed with HTTP {(int)response.StatusCode}";

        logger.LogError("Facebook API failure | Operation: {Operation} | Status: {Status} | Error: {Error}",
            operation, (int)response.StatusCode, error?.Message ?? body);

        throw new FacebookApiException(message, (int)response.StatusCode, error);
    }
}
