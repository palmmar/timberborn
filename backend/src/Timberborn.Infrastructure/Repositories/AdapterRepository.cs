using Microsoft.EntityFrameworkCore;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;
using Timberborn.Infrastructure.Data;

namespace Timberborn.Infrastructure.Repositories;

public class AdapterRepository : IAdapterRepository
{
    private readonly AppDbContext _db;
    public AdapterRepository(AppDbContext db) => _db = db;

    public Task<List<Adapter>> GetAllAsync() =>
        _db.Adapters.OrderBy(a => a.Name).ToListAsync();

    public Task<Adapter?> GetByIdAsync(Guid id) =>
        _db.Adapters.FirstOrDefaultAsync(a => a.Id == id);

    public Task<Adapter?> GetBySlugAsync(string slug) =>
        _db.Adapters.FirstOrDefaultAsync(a => a.Slug == slug);

    public async Task<Adapter> CreateAsync(Adapter adapter)
    {
        _db.Adapters.Add(adapter);
        await _db.SaveChangesAsync();
        return adapter;
    }

    public async Task<Adapter> UpdateAsync(Adapter adapter)
    {
        _db.Adapters.Update(adapter);
        await _db.SaveChangesAsync();
        return adapter;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Adapters.FindAsync(id);
        if (entity is not null)
        {
            _db.Adapters.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null) =>
        _db.Adapters.AnyAsync(a => a.Slug == slug && (excludeId == null || a.Id != excludeId));
}
