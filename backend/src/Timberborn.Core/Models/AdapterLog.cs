namespace Timberborn.Core.Models;

public class AdapterLog
{
    public Guid Id { get; set; }
    public Guid AdapterId { get; set; }
    public Adapter Adapter { get; set; } = null!;
    public string RawPayload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool TriggeredAnyRule { get; set; }
    public string? State { get; set; } // "on" | "off" | null
}
