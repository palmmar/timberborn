using Microsoft.EntityFrameworkCore;
using Timberborn.Core.Interfaces;
using Timberborn.Core.Models;
using Timberborn.Infrastructure.Data;

namespace Timberborn.Infrastructure.Repositories;

public class ProgramRepository : IProgramRepository
{
    private readonly AppDbContext _db;
    public ProgramRepository(AppDbContext db) => _db = db;

    public Task<List<AutomationProgram>> GetAllAsync() =>
        _db.Programs.OrderBy(p => p.Name).ToListAsync();

    public Task<List<AutomationProgram>> GetEnabledAsync() =>
        _db.Programs.Where(p => p.IsEnabled).ToListAsync();

    public Task<AutomationProgram?> GetByIdAsync(Guid id) =>
        _db.Programs.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<AutomationProgram> CreateAsync(AutomationProgram program)
    {
        _db.Programs.Add(program);
        await _db.SaveChangesAsync();
        return program;
    }

    public async Task<AutomationProgram> UpdateAsync(AutomationProgram program)
    {
        _db.Programs.Update(program);
        await _db.SaveChangesAsync();
        return program;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Programs.FindAsync(id);
        if (entity is not null)
        {
            _db.Programs.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
