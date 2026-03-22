namespace Timberborn.Core.Models;

public class ActionLog
{
    public Guid Id { get; set; }
    public Guid LeverId { get; set; }
    public Lever Lever { get; set; } = null!;
    public Guid? ProgramId { get; set; }
    public Guid? AdapterLogId { get; set; }
    public string? RequestBody { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CalledAt { get; set; }
    public string Source { get; set; } = "Manual";
}
