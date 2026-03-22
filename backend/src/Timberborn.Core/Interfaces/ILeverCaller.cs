namespace Timberborn.Core.Interfaces;

public record LeverCallResult(bool Success, int? StatusCode, string? Body, string? ErrorMessage);

public interface ILeverCaller
{
    Task<LeverCallResult> CallAsync(Models.Lever lever, string state); // state = "on" | "off"
}
