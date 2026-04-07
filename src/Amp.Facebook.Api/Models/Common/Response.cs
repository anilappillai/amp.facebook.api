namespace Amp.Facebook.Api.Models.Common;

public class Response
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public int? AffectedRows { get; set; }
}
