using Timberborn.Core.Interfaces;

namespace Timberborn.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard", async (
            IAdapterRepository adapters,
            ILeverRepository levers,
            IProgramRepository programs,
            IAdapterLogRepository adapterLogs,
            IActionLogRepository actionLogs) =>
        {
            var allAdapters = await adapters.GetAllAsync();
            var allLevers = await levers.GetAllAsync();
            var allPrograms = await programs.GetAllAsync();
            var adapterLogCount = await adapterLogs.CountAsync();
            var actionLogCount = await actionLogs.CountAsync();
            var recentAdapterLogs = await adapterLogs.GetRecentAsync(10);
            var recentActionLogs = await actionLogs.GetRecentAsync(10);

            return Results.Ok(new
            {
                counts = new
                {
                    adapters = allAdapters.Count,
                    levers = allLevers.Count,
                    programs = allPrograms.Count,
                    adapterLogs = adapterLogCount,
                    actionLogs = actionLogCount
                },
                recentAdapterLogs,
                recentActionLogs
            });
        });
    }
}
