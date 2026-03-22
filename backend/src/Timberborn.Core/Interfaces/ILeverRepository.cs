using Timberborn.Core.Models;

namespace Timberborn.Core.Interfaces;

public interface ILeverRepository
{
    Task<List<Lever>> GetAllAsync();
    Task<Lever?> GetByIdAsync(Guid id);
    Task<Lever> CreateAsync(Lever lever);
    Task<Lever> UpdateAsync(Lever lever);
    Task DeleteAsync(Guid id);
}
