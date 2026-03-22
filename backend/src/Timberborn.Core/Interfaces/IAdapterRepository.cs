using Timberborn.Core.Models;

namespace Timberborn.Core.Interfaces;

public interface IAdapterRepository
{
    Task<List<Adapter>> GetAllAsync();
    Task<Adapter?> GetByIdAsync(Guid id);
    Task<Adapter?> GetBySlugAsync(string slug);
    Task<Adapter> CreateAsync(Adapter adapter);
    Task<Adapter> UpdateAsync(Adapter adapter);
    Task DeleteAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
}
