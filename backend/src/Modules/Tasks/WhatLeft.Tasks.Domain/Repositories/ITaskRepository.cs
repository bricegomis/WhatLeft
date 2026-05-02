using WhatLeft.Tasks.Domain.Entities;

namespace WhatLeft.Tasks.Domain.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetHistoryAsync(string userId, CancellationToken ct = default);
    Task<int> CountByTemplateAndPeriodAsync(Guid templateId, DateTimeOffset periodStart, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetUnfinishedByTemplateAndPeriodAsync(Guid templateId, DateTimeOffset periodStart, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskItem?> GetByIdForUserAsync(Guid id, string userId, CancellationToken ct = default);
    Task AddAsync(TaskItem task, CancellationToken ct = default);
    void Update(TaskItem task);
    void Remove(TaskItem task);
    Task SaveChangesAsync(CancellationToken ct = default);
}
