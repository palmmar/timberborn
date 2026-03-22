namespace Timberborn.Core.Models;

public class Adapter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<AdapterLog> Logs { get; set; } = [];
}
