using Timberborn.Core.Models;

namespace Timberborn.Core.Interfaces;

public interface IProgramRepository
{
    Task<List<AutomationProgram>> GetAllAsync();
    Task<List<AutomationProgram>> GetEnabledAsync();
    Task<AutomationProgram?> GetByIdAsync(Guid id);
    Task<AutomationProgram> CreateAsync(AutomationProgram program);
    Task<AutomationProgram> UpdateAsync(AutomationProgram program);
    Task DeleteAsync(Guid id);
}
