using Microsoft.EntityFrameworkCore;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;
using Timberborn.Infrastructure.Data;

namespace Timberborn.Infrastructure.Repositories;

public class AdapterLogRepository : IAdapterLogRepository
{
    private readonly AppDbContext _db;
    public AdapterLogRepository(AppDbContext db) => _db = db;

    public async Task<(List<AdapterLog> Items, int Total)> GetPagedAsync(Guid? adapterId, int page, int pageSize)
    {
        var query = _db.AdapterLogs.Include(l => l.Adapter).AsQueryable();
        if (adapterId.HasValue)
            query = query.Where(l => l.AdapterId == adapterId.Value);
        query = query.OrderByDescending(l => l.ReceivedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<AdapterLog?> GetByIdAsync(Guid id) =>
        _db.AdapterLogs.Include(l => l.Adapter).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<AdapterLog> CreateAsync(AdapterLog log)
    {
        _db.AdapterLogs.Add(log);
        await _db.SaveChangesAsync();
        return log;
    }

    public async Task UpdateAsync(AdapterLog log)
    {
        _db.AdapterLogs.Update(log);
        await _db.SaveChangesAsync();
    }

    public async Task PurgeAllAsync()
    {
        await _db.AdapterLogs.ExecuteDeleteAsync();
    }

    public Task<int> CountAsync() => _db.AdapterLogs.CountAsync();

    public Task<List<AdapterLog>> GetRecentAsync(int count) =>
        _db.AdapterLogs.Include(l => l.Adapter)
            .OrderByDescending(l => l.ReceivedAt)
            .Take(count)
            .ToListAsync();
}
