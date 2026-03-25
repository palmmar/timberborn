using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;
using Timberborn.Infrastructure.Services;

namespace Timberborn.Api.Endpoints;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/adapters/in/{slug}", (string slug, HttpRequest request, IAdapterRepository adapters,
            IAdapterLogRepository adapterLogs, ProgramEngine engine) =>
            HandleAsync(slug, null, request, adapters, adapterLogs, engine));

        app.MapPost("/adapters/in/{slug}/on", (string slug, HttpRequest request, IAdapterRepository adapters,
            IAdapterLogRepository adapterLogs, ProgramEngine engine) =>
            HandleAsync(slug, "on", request, adapters, adapterLogs, engine));

        app.MapPost("/adapters/in/{slug}/off", (string slug, HttpRequest request, IAdapterRepository adapters,
            IAdapterLogRepository adapterLogs, ProgramEngine engine) =>
            HandleAsync(slug, "off", request, adapters, adapterLogs, engine));
    }

    private static async Task<IResult> HandleAsync(
        string slug, string? state, HttpRequest request,
        IAdapterRepository adapters, IAdapterLogRepository adapterLogs, ProgramEngine engine)
    {
        var adapter = await adapters.GetBySlugAsync(slug);
        if (adapter is null || !adapter.IsEnabled)
            return Results.NotFound(new { error = $"Adapter '{slug}' not found or disabled" });

        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body)) body = "{}";

        var adapterLog = await adapterLogs.CreateAsync(new AdapterLog
        {
            Id = Guid.NewGuid(),
            AdapterId = adapter.Id,
            RawPayload = body,
            ReceivedAt = DateTime.UtcNow,
            TriggeredAnyRule = false,
            State = state
        });

        await engine.HandleSignalAsync(adapter, state, adapterLog.Id);

        adapterLog.TriggeredAnyRule = state is not null;
        await adapterLogs.UpdateAsync(adapterLog);

        if (state is not null)
        {
            adapter.LastState = state;
            await adapters.UpdateAsync(adapter);
            await engine.BroadcastSignalUpdatesAsync();
        }

        return Results.Ok(new { adapterLogId = adapterLog.Id, triggeredAnyRule = adapterLog.TriggeredAnyRule });
    }
}
