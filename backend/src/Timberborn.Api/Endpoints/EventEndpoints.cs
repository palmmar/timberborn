using System.Net.ServerSentEvents;
using System.Text.Json;
using Timberborn.Core.Interfaces;

namespace Timberborn.Api.Endpoints;

public static class EventEndpoints
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events", (ILogBroadcaster broadcaster, CancellationToken ct) =>
        {
            var stream = broadcaster.Subscribe(ct)
                .Select(evt => new SseItem<string>(
                    JsonSerializer.Serialize(evt.Data, _jsonOptions),
                    eventType: evt.Type));

            return TypedResults.ServerSentEvents(stream);
        });
    }
}
