using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Timberborn.Core.Interfaces;

namespace Timberborn.Infrastructure.Services;

public class LogBroadcaster : ILogBroadcaster
{
    private readonly List<Channel<LogEvent>> _channels = [];
    private readonly Lock _lock = new();

    public void Publish(LogEvent evt)
    {
        List<Channel<LogEvent>> snapshot;
        lock (_lock) { snapshot = [.._channels]; }
        foreach (var ch in snapshot)
            ch.Writer.TryWrite(evt);
    }

    public async IAsyncEnumerable<LogEvent> Subscribe([EnumeratorCancellation] CancellationToken ct)
    {
        var channel = Channel.CreateUnbounded<LogEvent>();
        lock (_lock) { _channels.Add(channel); }
        try
        {
            await foreach (var evt in channel.Reader.ReadAllAsync(ct))
                yield return evt;
        }
        finally
        {
            lock (_lock) { _channels.Remove(channel); }
        }
    }
}
