using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Timberborn.Core.Interfaces;
using Timberborn.Infrastructure.Data;
using Timberborn.Infrastructure.Repositories;
using Timberborn.Infrastructure.Services;

namespace Timberborn.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connectionString));

        services.AddScoped<IAdapterRepository, AdapterRepository>();
        services.AddScoped<ILeverRepository, LeverRepository>();
        services.AddScoped<IProgramRepository, ProgramRepository>();
        services.AddScoped<IAdapterLogRepository, AdapterLogRepository>();
        services.AddScoped<IActionLogRepository, ActionLogRepository>();

        services.AddHttpClient<ILeverCaller, LeverCaller>();

        services.AddSingleton<ILogBroadcaster, LogBroadcaster>();
        services.AddSingleton<ProgramEngine>();

        return services;
    }
}
