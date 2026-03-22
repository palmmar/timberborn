using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;

namespace Timberborn.Api.Endpoints;

public static class LeverEndpoints
{
    public static void MapLeverEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/levers");

        group.MapGet("/", async (ILeverRepository repo) =>
            Results.Ok(await repo.GetAllAsync()));

        group.MapGet("/{id:guid}", async (Guid id, ILeverRepository repo) =>
        {
            var lever = await repo.GetByIdAsync(id);
            return lever is null ? Results.NotFound() : Results.Ok(lever);
        });

        group.MapPost("/", async (LeverRequest req, ILeverRepository repo) =>
        {
            var lever = new Lever
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                UrlOn = req.UrlOn,
                UrlOff = req.UrlOff,
                HttpMethod = req.HttpMethod ?? "POST",
                BodyTemplate = req.BodyTemplate,
                Description = req.Description,
                IsEnabled = req.IsEnabled ?? true,
                CreatedAt = DateTime.UtcNow
            };
            var created = await repo.CreateAsync(lever);
            return Results.Created($"/api/levers/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (Guid id, LeverRequest req, ILeverRepository repo) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Results.NotFound();
            existing.Name = req.Name;
            existing.UrlOn = req.UrlOn;
            existing.UrlOff = req.UrlOff;
            existing.HttpMethod = req.HttpMethod ?? existing.HttpMethod;
            existing.BodyTemplate = req.BodyTemplate;
            existing.Description = req.Description;
            existing.IsEnabled = req.IsEnabled ?? existing.IsEnabled;
            return Results.Ok(await repo.UpdateAsync(existing));
        });

        group.MapDelete("/{id:guid}", async (Guid id, ILeverRepository repo) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Results.NotFound();
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/trigger", async (
            Guid id,
            string? state,
            ILeverRepository repo,
            ILeverCaller caller,
            IActionLogRepository actionLogs,
            ILogBroadcaster broadcaster) =>
        {
            var lever = await repo.GetByIdAsync(id);
            if (lever is null) return Results.NotFound();
            if (!lever.IsEnabled) return Results.BadRequest(new { error = "Lever is disabled" });

            var result = await caller.CallAsync(lever, state ?? "on");
            var log = await actionLogs.CreateAsync(new ActionLog
            {
                Id = Guid.NewGuid(),
                LeverId = lever.Id,
                RequestBody = lever.BodyTemplate,
                ResponseStatusCode = result.StatusCode,
                ResponseBody = result.Body,
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                CalledAt = DateTime.UtcNow,
                Source = "Manual"
            });
            broadcaster.Publish(new LogEvent("action_log", log));
            return Results.Ok(log);
        });
    }

    private record LeverRequest(string Name, string? UrlOn, string? UrlOff, string? HttpMethod, string? BodyTemplate, string? Description, bool? IsEnabled);
}
