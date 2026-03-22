namespace Timberborn.Core.Models;

public class Lever
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UrlOn  { get; set; }
    public string? UrlOff { get; set; }
    public string HttpMethod { get; set; } = "POST";
    public string? BodyTemplate { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
