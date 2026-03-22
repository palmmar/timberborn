using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;

namespace Timberborn.Api.Endpoints;

public static class AdapterEndpoints
{
    public static void MapAdapterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/adapters");

        group.MapGet("/", async (IAdapterRepository repo) =>
            Results.Ok(await repo.GetAllAsync()));

        group.MapGet("/{id:guid}", async (Guid id, IAdapterRepository repo) =>
        {
            var adapter = await repo.GetByIdAsync(id);
            return adapter is null ? Results.NotFound() : Results.Ok(adapter);
        });

        group.MapPost("/", async (AdapterRequest req, IAdapterRepository repo) =>
        {
            if (await repo.SlugExistsAsync(req.Slug))
                return Results.Conflict(new { error = "Slug already exists" });

            var adapter = new Adapter
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Slug = req.Slug,
                Description = req.Description,
                IsEnabled = req.IsEnabled ?? true,
                CreatedAt = DateTime.UtcNow
            };
            var created = await repo.CreateAsync(adapter);
            return Results.Created($"/api/adapters/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (Guid id, AdapterRequest req, IAdapterRepository repo) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Results.NotFound();

            if (await repo.SlugExistsAsync(req.Slug, id))
                return Results.Conflict(new { error = "Slug already exists" });

            existing.Name = req.Name;
            existing.Slug = req.Slug;
            existing.Description = req.Description;
            existing.IsEnabled = req.IsEnabled ?? existing.IsEnabled;
            return Results.Ok(await repo.UpdateAsync(existing));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IAdapterRepository repo) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Results.NotFound();
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });
    }

    private record AdapterRequest(string Name, string Slug, string? Description, bool? IsEnabled);
}
