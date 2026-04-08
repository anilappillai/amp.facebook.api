namespace Amp.Facebook.Api.Models.Common;

/// <summary>Generic API response envelope used for simple success/failure results.</summary>
public class Response
{
    /// <summary>Indicates whether the operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Human-readable message describing the outcome.</summary>
    public string? Message { get; set; }

    /// <summary>Optional payload returned with the response.</summary>
    public object? Data { get; set; }

    /// <summary>Number of rows affected, where applicable.</summary>
    public int? AffectedRows { get; set; }
}
