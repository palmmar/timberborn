namespace Timberborn.Core.Interfaces;

public record LogEvent(string Type, object Data);

public interface ILogBroadcaster
{
    void Publish(LogEvent evt);
    IAsyncEnumerable<LogEvent> Subscribe(CancellationToken ct);
}
