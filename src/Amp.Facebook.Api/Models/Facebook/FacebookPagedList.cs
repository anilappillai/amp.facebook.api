using System.Text.Json.Serialization;

namespace Amp.Facebook.Api.Models.Facebook;

/// <summary>Graph API paged list wrapper (data + paging cursor).</summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class FacebookPagedList<T>
{
    /// <summary>The items in the current page of results.</summary>
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];

    /// <summary>Paging cursors for fetching additional pages.</summary>
    [JsonPropertyName("paging")]
    public FacebookPaging? Paging { get; set; }
}
