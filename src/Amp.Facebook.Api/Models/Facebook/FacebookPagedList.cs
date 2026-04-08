using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Graph API paged list wrapper (data + paging cursor).</summary>
public sealed class FacebookPagedList<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];

    [JsonPropertyName("paging")]
    public FacebookPaging? Paging { get; set; }
}
