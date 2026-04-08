using Amp.Facebook.Api.Models.Facebook;
using Amp.Facebook.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Amp.Facebook.Api.Tests.Services;

public class FacebookServiceTests
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (FacebookService service, Mock<HttpMessageHandler> handler) BuildSut(
        HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://graph.facebook.com/v25.0/")
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("FacebookGraph")).Returns(httpClient);

        var service = new FacebookService(factory.Object, NullLogger<FacebookService>.Instance);
        return (service, handler);
    }

    private static HttpResponseMessage Json(object payload, HttpStatusCode status = HttpStatusCode.OK)
        => new(status)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json")
        };

    // ── GetUserPagesAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserPagesAsync_Success_ReturnsPages()
    {
        var payload = new
        {
            data = new[]
            {
                new { id = "123", name = "Test Page", access_token = "page-token", category = "Brand", tasks = Array.Empty<string>() }
            }
        };
        var (sut, _) = BuildSut(Json(payload));

        var pages = await sut.GetUserPagesAsync("user-token");

        Assert.Single(pages);
        Assert.Equal("123", pages[0].Id);
        Assert.Equal("Test Page", pages[0].Name);
    }

    [Fact]
    public async Task GetUserPagesAsync_EmptyData_ReturnsEmptyList()
    {
        var (sut, _) = BuildSut(Json(new { data = Array.Empty<object>() }));

        var pages = await sut.GetUserPagesAsync("user-token");

        Assert.Empty(pages);
    }

    [Fact]
    public async Task GetUserPagesAsync_FacebookError_ThrowsFacebookApiException()
    {
        var errorPayload = new
        {
            error = new { message = "Invalid OAuth access token.", type = "OAuthException", code = 190 }
        };
        var (sut, _) = BuildSut(Json(errorPayload, HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<FacebookApiException>(
            () => sut.GetUserPagesAsync("bad-token"));
    }

    // ── CreatePostAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePostAsync_Success_ReturnsCreatedResult()
    {
        var (sut, _) = BuildSut(Json(new { id = "123_456" }));

        var result = await sut.CreatePostAsync(
            "123", "page-token", new PostToPageRequest { Message = "Hello!" });

        Assert.Equal("123_456", result.Id);
    }

    [Fact]
    public async Task CreatePostAsync_FacebookError_ThrowsFacebookApiException()
    {
        var errorPayload = new
        {
            error = new { message = "Pages publish not enabled.", type = "GraphMethodException", code = 200 }
        };
        var (sut, _) = BuildSut(Json(errorPayload, HttpStatusCode.Forbidden));

        await Assert.ThrowsAsync<FacebookApiException>(
            () => sut.CreatePostAsync("123", "page-token", new PostToPageRequest { Message = "Hello!" }));
    }

    // ── DeletePostAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePostAsync_Success_ReturnsTrue()
    {
        var (sut, _) = BuildSut(Json(new { success = true }));

        var result = await sut.DeletePostAsync("123_456", "page-token");

        Assert.True(result);
    }

    [Fact]
    public async Task DeletePostAsync_SuccessFalse_ReturnsFalse()
    {
        var (sut, _) = BuildSut(Json(new { success = false }));

        var result = await sut.DeletePostAsync("123_456", "page-token");

        Assert.False(result);
    }

    // ── UploadPhotoAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UploadPhotoAsync_ByUrl_ReturnsCreatedResult()
    {
        var (sut, _) = BuildSut(Json(new { id = "photo-123" }));

        var result = await sut.UploadPhotoAsync(
            "123", "page-token",
            new UploadPhotoRequest { Url = "https://example.com/photo.jpg" });

        Assert.Equal("photo-123", result.Id);
    }

    [Fact]
    public async Task UploadPhotoAsync_ByBase64_ReturnsCreatedResult()
    {
        var (sut, _) = BuildSut(Json(new { id = "photo-456" }));

        // Minimal valid 1x1 white JPEG in base64
        var base64 = Convert.ToBase64String(new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 });

        var result = await sut.UploadPhotoAsync(
            "123", "page-token",
            new UploadPhotoRequest { Base64 = base64 });

        Assert.Equal("photo-456", result.Id);
    }

    [Fact]
    public async Task UploadPhotoAsync_NoUrlOrBase64_ThrowsArgumentException()
    {
        var (sut, _) = BuildSut(Json(new { id = "photo-456" }));

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.UploadPhotoAsync("123", "page-token", new UploadPhotoRequest()));
    }

    // ── DeletePhotoAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePhotoAsync_Success_ReturnsTrue()
    {
        var (sut, _) = BuildSut(Json(new { success = true }));

        var result = await sut.DeletePhotoAsync("photo-123", "page-token");

        Assert.True(result);
    }

    [Fact]
    public async Task DeletePhotoAsync_FacebookError_ThrowsFacebookApiException()
    {
        var errorPayload = new
        {
            error = new { message = "Unsupported delete request.", type = "GraphMethodException", code = 100 }
        };
        var (sut, _) = BuildSut(Json(errorPayload, HttpStatusCode.BadRequest));

        await Assert.ThrowsAsync<FacebookApiException>(
            () => sut.DeletePhotoAsync("photo-123", "page-token"));
    }
}
