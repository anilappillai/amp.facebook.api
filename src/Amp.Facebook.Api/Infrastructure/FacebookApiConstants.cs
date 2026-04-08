namespace Amp.Facebook.Api.Infrastructure;

/// <summary>Constant values used across the Facebook API integration layer.</summary>
internal static class FacebookApiConstants
{
    /// <summary>Named HttpClient key registered in DI.</summary>
    public const string HttpClientName = "FacebookGraph";

    /// <summary>HTTP authentication scheme for Facebook token headers.</summary>
    public const string AuthScheme = "Bearer";

    // ── Request headers ───────────────────────────────────────────────────────

    /// <summary>Header carrying the Facebook user access token.</summary>
    public const string HeaderUserAccessToken = "X-User-Access-Token";

    /// <summary>Header carrying the Facebook page access token.</summary>
    public const string HeaderPageAccessToken = "X-Page-Access-Token";

    // ── Graph API endpoints ───────────────────────────────────────────────────

    /// <summary>Endpoint for fetching pages managed by the authenticated user.</summary>
    public const string EndpointMeAccounts = "me/accounts?fields=id,name,access_token,category,tasks";

    // ── Form field keys ───────────────────────────────────────────────────────

    public const string FormMessage = "message";
    public const string FormLink = "link";
    public const string FormPublished = "published";
    public const string FormScheduledPublishTime = "scheduled_publish_time";
    public const string FormUrl = "url";
    public const string FormCaption = "caption";
    public const string FormSource = "source";
    public const string FormPhotoFileName = "photo.jpg";

    // ── Error messages ────────────────────────────────────────────────────────

    public const string ErrorUserTokenRequired = "X-User-Access-Token header is required.";
    public const string ErrorPageTokenRequired = "X-Page-Access-Token header is required.";
    public const string ErrorMessageRequired = "Message is required.";
    public const string ErrorUrlOrBase64Required = "Either Url or Base64 must be provided.";
    public const string ErrorDeletionNotConfirmed = "Facebook did not confirm deletion.";
    public const string ErrorEmptyPostResponse = "Empty response from Facebook when creating post.";
    public const string ErrorEmptyPhotoResponse = "Empty response from Facebook when uploading photo.";
    public const string ErrorUrlOrBase64RequiredService = "Either Url or Base64 must be provided for photo upload.";
}
