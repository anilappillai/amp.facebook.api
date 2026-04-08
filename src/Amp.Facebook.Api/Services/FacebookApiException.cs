using Amp.Facebook.Api.Models.Facebook;

namespace Amp.Facebook.Api.Services;

/// <summary>Thrown when the Facebook Graph API returns an error response.</summary>
public sealed class FacebookApiException(
    string message,
    int httpStatusCode = 500,
    FacebookApiError? apiError = null) : Exception(message)
{
    public int HttpStatusCode { get; } = httpStatusCode;
    public FacebookApiError? ApiError { get; } = apiError;
}
