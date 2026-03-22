using Timberborn.Core.Models;

namespace Timberborn.Core.Interfaces;

public interface IAdapterLogRepository
{
    Task<(List<AdapterLog> Items, int Total)> GetPagedAsync(Guid? adapterId, int page, int pageSize);
    Task<AdapterLog?> GetByIdAsync(Guid id);
    Task<AdapterLog> CreateAsync(AdapterLog log);
    Task UpdateAsync(AdapterLog log);
    Task PurgeAllAsync();
    Task<int> CountAsync();
    Task<List<AdapterLog>> GetRecentAsync(int count);
}
