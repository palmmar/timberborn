using Timberborn.Core.Interfaces;

namespace Timberborn.Api.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this IEndpointRouteBuilder app)
    {
        var adapterLogs = app.MapGroup("/api/logs/adapter");
        var actionLogs = app.MapGroup("/api/logs/action");

        adapterLogs.MapGet("/", async (
            Guid? adapterId, int page, int pageSize,
            IAdapterLogRepository repo) =>
        {
            var (items, total) = await repo.GetPagedAsync(adapterId, page < 1 ? 1 : page, pageSize < 1 ? 20 : pageSize);
            return Results.Ok(new { items, total });
        });

        adapterLogs.MapGet("/{id:guid}", async (Guid id, IAdapterLogRepository repo) =>
        {
            var log = await repo.GetByIdAsync(id);
            return log is null ? Results.NotFound() : Results.Ok(log);
        });

        adapterLogs.MapDelete("/", async (IAdapterLogRepository repo) =>
        {
            await repo.PurgeAllAsync();
            return Results.NoContent();
        });

        actionLogs.MapGet("/", async (
            Guid? leverId, string? source, int page, int pageSize,
            IActionLogRepository repo) =>
        {
            var (items, total) = await repo.GetPagedAsync(leverId, source, page < 1 ? 1 : page, pageSize < 1 ? 20 : pageSize);
            return Results.Ok(new { items, total });
        });

        actionLogs.MapGet("/{id:guid}", async (Guid id, IActionLogRepository repo) =>
        {
            var log = await repo.GetByIdAsync(id);
            return log is null ? Results.NotFound() : Results.Ok(log);
        });

        actionLogs.MapDelete("/", async (IActionLogRepository repo) =>
        {
            await repo.PurgeAllAsync();
            return Results.NoContent();
        });
    }
}
