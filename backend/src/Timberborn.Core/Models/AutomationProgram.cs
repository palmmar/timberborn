namespace Timberborn.Core.Models;

public class AutomationProgram
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string GraphJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
