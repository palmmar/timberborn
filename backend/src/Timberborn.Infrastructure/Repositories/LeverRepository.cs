using Microsoft.EntityFrameworkCore;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;
using Timberborn.Infrastructure.Data;

namespace Timberborn.Infrastructure.Repositories;

public class LeverRepository : ILeverRepository
{
    private readonly AppDbContext _db;
    public LeverRepository(AppDbContext db) => _db = db;

    public Task<List<Lever>> GetAllAsync() =>
        _db.Levers.OrderBy(l => l.Name).ToListAsync();

    public Task<Lever?> GetByIdAsync(Guid id) =>
        _db.Levers.FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Lever> CreateAsync(Lever lever)
    {
        _db.Levers.Add(lever);
        await _db.SaveChangesAsync();
        return lever;
    }

    public async Task<Lever> UpdateAsync(Lever lever)
    {
        _db.Levers.Update(lever);
        await _db.SaveChangesAsync();
        return lever;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Levers.FindAsync(id);
        if (entity is not null)
        {
            _db.Levers.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
