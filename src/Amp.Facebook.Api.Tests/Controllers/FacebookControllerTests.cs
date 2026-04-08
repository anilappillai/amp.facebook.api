using Amp.Facebook.Api.Controllers;
using Amp.Facebook.Api.Models.Facebook;
using Amp.Facebook.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Amp.Facebook.Api.Tests.Controllers;

public class FacebookControllerTests
{
    private readonly Mock<IFacebookService> _service = new();
    private readonly FacebookController _sut;

    public FacebookControllerTests()
    {
        _sut = new FacebookController(_service.Object, NullLogger<FacebookController>.Instance);
    }

    // ── GetPagesAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagesAsync_MissingToken_ReturnsBadRequest()
    {
        var result = await _sut.GetPagesAsync(string.Empty, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(bad.Value);
    }

    [Fact]
    public async Task GetPagesAsync_ValidToken_ReturnsOkWithPages()
    {
        var pages = new List<FacebookPageInfo>
        {
            new() { Id = "123", Name = "Test Page", AccessToken = "page-token" }
        };
        _service.Setup(s => s.GetUserPagesAsync("user-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);

        var result = await _sut.GetPagesAsync("user-token", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(pages, ok.Value);
    }

    // ── CreatePostAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePostAsync_MissingToken_ReturnsBadRequest()
    {
        var result = await _sut.CreatePostAsync(
            "page-1", string.Empty, new PostToPageRequest { Message = "Hello" }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreatePostAsync_MissingMessage_ReturnsBadRequest()
    {
        var result = await _sut.CreatePostAsync(
            "page-1", "page-token", new PostToPageRequest { Message = string.Empty }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreatePostAsync_ValidRequest_Returns201WithResult()
    {
        var request = new PostToPageRequest { Message = "Hello world" };
        var created = new FacebookCreateResult { Id = "123_456" };
        _service.Setup(s => s.CreatePostAsync("page-1", "page-token", request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

        var result = await _sut.CreatePostAsync("page-1", "page-token", request, CancellationToken.None);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, status.StatusCode);
        Assert.Equal(created, status.Value);
    }

    // ── DeletePostAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePostAsync_MissingToken_ReturnsBadRequest()
    {
        var result = await _sut.DeletePostAsync("post-1", string.Empty, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeletePostAsync_ServiceReturnsFalse_Returns502()
    {
        _service.Setup(s => s.DeletePostAsync("post-1", "page-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        var result = await _sut.DeletePostAsync("post-1", "page-token", CancellationToken.None);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status502BadGateway, status.StatusCode);
    }

    [Fact]
    public async Task DeletePostAsync_ServiceReturnsTrue_Returns204()
    {
        _service.Setup(s => s.DeletePostAsync("post-1", "page-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

        var result = await _sut.DeletePostAsync("post-1", "page-token", CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    // ── UploadPhotoAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UploadPhotoAsync_MissingToken_ReturnsBadRequest()
    {
        var result = await _sut.UploadPhotoAsync(
            "page-1", string.Empty, new UploadPhotoRequest { Url = "https://example.com/photo.jpg" }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadPhotoAsync_NoUrlOrBase64_ReturnsBadRequest()
    {
        var result = await _sut.UploadPhotoAsync(
            "page-1", "page-token", new UploadPhotoRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadPhotoAsync_ValidUrlRequest_Returns201WithResult()
    {
        var request = new UploadPhotoRequest { Url = "https://example.com/photo.jpg" };
        var created = new FacebookCreateResult { Id = "photo-123" };
        _service.Setup(s => s.UploadPhotoAsync("page-1", "page-token", request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

        var result = await _sut.UploadPhotoAsync("page-1", "page-token", request, CancellationToken.None);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, status.StatusCode);
        Assert.Equal(created, status.Value);
    }

    // ── DeletePhotoAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePhotoAsync_MissingToken_ReturnsBadRequest()
    {
        var result = await _sut.DeletePhotoAsync("photo-1", string.Empty, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeletePhotoAsync_ServiceReturnsFalse_Returns502()
    {
        _service.Setup(s => s.DeletePhotoAsync("photo-1", "page-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        var result = await _sut.DeletePhotoAsync("photo-1", "page-token", CancellationToken.None);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status502BadGateway, status.StatusCode);
    }

    [Fact]
    public async Task DeletePhotoAsync_ServiceReturnsTrue_Returns204()
    {
        _service.Setup(s => s.DeletePhotoAsync("photo-1", "page-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

        var result = await _sut.DeletePhotoAsync("photo-1", "page-token", CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
