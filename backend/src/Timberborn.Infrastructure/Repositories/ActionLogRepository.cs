using Microsoft.EntityFrameworkCore;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;
using Timberborn.Infrastructure.Data;

namespace Timberborn.Infrastructure.Repositories;

public class ActionLogRepository : IActionLogRepository
{
    private readonly AppDbContext _db;
    public ActionLogRepository(AppDbContext db) => _db = db;

    public async Task<(List<ActionLog> Items, int Total)> GetPagedAsync(Guid? leverId, string? source, int page, int pageSize)
    {
        var query = _db.ActionLogs.Include(l => l.Lever).AsQueryable();
        if (leverId.HasValue)
            query = query.Where(l => l.LeverId == leverId.Value);
        if (!string.IsNullOrWhiteSpace(source))
            query = query.Where(l => l.Source == source);
        query = query.OrderByDescending(l => l.CalledAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<ActionLog?> GetByIdAsync(Guid id) =>
        _db.ActionLogs.Include(l => l.Lever).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<ActionLog> CreateAsync(ActionLog log)
    {
        _db.ActionLogs.Add(log);
        await _db.SaveChangesAsync();
        return log;
    }

    public async Task PurgeAllAsync()
    {
        await _db.ActionLogs.ExecuteDeleteAsync();
    }

    public Task<int> CountAsync() => _db.ActionLogs.CountAsync();

    public Task<List<ActionLog>> GetRecentAsync(int count) =>
        _db.ActionLogs.Include(l => l.Lever)
            .OrderByDescending(l => l.CalledAt)
            .Take(count)
            .ToListAsync();
}
