using Timberborn.Core.Models;

namespace Timberborn.Core.Interfaces;

public interface IActionLogRepository
{
    Task<(List<ActionLog> Items, int Total)> GetPagedAsync(Guid? leverId, string? source, int page, int pageSize);
    Task<ActionLog?> GetByIdAsync(Guid id);
    Task<ActionLog> CreateAsync(ActionLog log);
    Task PurgeAllAsync();
    Task<int> CountAsync();
    Task<List<ActionLog>> GetRecentAsync(int count);
}
