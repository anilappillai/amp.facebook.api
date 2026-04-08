using Amp.Facebook.Api.Models.Facebook;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Amp.Facebook.Api.Services;

/// <summary>
/// Calls the Facebook Graph API v19.0 using a named <see cref="HttpClient"/>
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

    public async Task<List<FacebookPageInfo>> GetUserPagesAsync(
        string userAccessToken, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching Facebook pages for user");

        var client = CreateClient(userAccessToken);
        var response = await client.GetAsync("me/accounts?fields=id,name,access_token,category,tasks", ct);
        var body = await EnsureSuccessAsync(response, "GET /me/accounts", ct);

        var paged = JsonSerializer.Deserialize<FacebookPagedList<FacebookPageInfo>>(body, _json);
        return paged?.Data ?? [];
    }

    public async Task<FacebookCreateResult> CreatePostAsync(
        string pageId, string pageAccessToken, PostToPageRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Creating post on Facebook page {PageId}", pageId);

        var form = new Dictionary<string, string>
        {
            ["message"] = request.Message
        };

        if (!string.IsNullOrWhiteSpace(request.Link))
            form["link"] = request.Link;

        if (!request.Published)
        {
            form["published"] = "false";
            if (request.ScheduledPublishTime.HasValue)
                form["scheduled_publish_time"] = request.ScheduledPublishTime.Value.ToString();
        }

        var client = CreateClient(pageAccessToken);
        var response = await client.PostAsync($"{pageId}/feed", new FormUrlEncodedContent(form), ct);
        var body = await EnsureSuccessAsync(response, $"POST /{pageId}/feed", ct);

        return JsonSerializer.Deserialize<FacebookCreateResult>(body, _json)
               ?? throw new FacebookApiException("Empty response from Facebook when creating post.");
    }

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

    public async Task<FacebookCreateResult> UploadPhotoAsync(
        string pageId, string pageAccessToken, UploadPhotoRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Uploading photo to Facebook page {PageId}", pageId);

        var client = CreateClient(pageAccessToken);
        HttpResponseMessage response;

        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            // Upload by URL — Graph API fetches the image itself
            var form = new Dictionary<string, string> { ["url"] = request.Url };
            if (!string.IsNullOrWhiteSpace(request.Caption)) form["caption"] = request.Caption;
            form["published"] = request.Published ? "true" : "false";

            response = await client.PostAsync($"{pageId}/photos", new FormUrlEncodedContent(form), ct);
        }
        else if (!string.IsNullOrWhiteSpace(request.Base64))
        {
            // Upload raw image bytes
            var bytes = Convert.FromBase64String(request.Base64);
            var multipart = new MultipartFormDataContent();
            multipart.Add(new ByteArrayContent(bytes), "source", "photo.jpg");
            if (!string.IsNullOrWhiteSpace(request.Caption))
                multipart.Add(new StringContent(request.Caption), "caption");
            multipart.Add(new StringContent(request.Published ? "true" : "false"), "published");

            response = await client.PostAsync($"{pageId}/photos", multipart, ct);
        }
        else
        {
            throw new ArgumentException("Either Url or Base64 must be provided for photo upload.");
        }

        var body = await EnsureSuccessAsync(response, $"POST /{pageId}/photos", ct);
        return JsonSerializer.Deserialize<FacebookCreateResult>(body, _json)
               ?? throw new FacebookApiException("Empty response from Facebook when uploading photo.");
    }

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
        var client = httpClientFactory.CreateClient("FacebookGraph");
        // Facebook Graph API accepts the token as a Bearer token
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
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

