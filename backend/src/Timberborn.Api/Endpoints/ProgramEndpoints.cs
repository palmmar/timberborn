using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;

namespace Timberborn.Api.Endpoints;

public static class ProgramEndpoints
{
    public static void MapProgramEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/programs", async (IProgramRepository repo) =>
            Results.Ok(await repo.GetAllAsync()));

        app.MapGet("/api/programs/{id:guid}", async (Guid id, IProgramRepository repo) =>
        {
            var program = await repo.GetByIdAsync(id);
            return program is null ? Results.NotFound() : Results.Ok(program);
        });

        app.MapPost("/api/programs", async (CreateProgramRequest req, IProgramRepository repo) =>
        {
            var program = await repo.CreateAsync(new AutomationProgram
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                IsEnabled = req.IsEnabled,
                GraphJson = req.GraphJson ?? "{}",
                CreatedAt = DateTime.UtcNow
            });
            return Results.Created($"/api/programs/{program.Id}", program);
        });

        app.MapPut("/api/programs/{id:guid}", async (Guid id, UpdateProgramRequest req, IProgramRepository repo) =>
        {
            var program = await repo.GetByIdAsync(id);
            if (program is null) return Results.NotFound();
            program.Name = req.Name;
            program.IsEnabled = req.IsEnabled;
            program.GraphJson = req.GraphJson ?? "{}";
            return Results.Ok(await repo.UpdateAsync(program));
        });

        app.MapDelete("/api/programs/{id:guid}", async (Guid id, IProgramRepository repo) =>
        {
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });

        app.MapMethods("/api/programs/{id:guid}/enabled", ["PATCH"],
            async (Guid id, SetEnabledRequest req, IProgramRepository repo) =>
            {
                var program = await repo.GetByIdAsync(id);
                if (program is null) return Results.NotFound();
                program.IsEnabled = req.IsEnabled;
                return Results.Ok(await repo.UpdateAsync(program));
            });
    }

    private record CreateProgramRequest(string Name, bool IsEnabled, string? GraphJson);
    private record UpdateProgramRequest(string Name, bool IsEnabled, string? GraphJson);
    private record SetEnabledRequest(bool IsEnabled);
}
