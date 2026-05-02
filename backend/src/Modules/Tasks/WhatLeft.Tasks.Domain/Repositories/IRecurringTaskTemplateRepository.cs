using WhatLeft.Tasks.Domain.Entities;

namespace WhatLeft.Tasks.Domain.Repositories;

public interface IRecurringTaskTemplateRepository
{
    /// <summary>Returns all active templates (background service use — no user filter).</summary>
    Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Returns templates owned by a specific user.</summary>
    Task<IEnumerable<RecurringTaskTemplate>> GetAllForUserAsync(string userId, CancellationToken ct = default);
    Task<RecurringTaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RecurringTaskTemplate?> GetByIdForUserAsync(Guid id, string userId, CancellationToken ct = default);
    Task AddAsync(RecurringTaskTemplate template, CancellationToken ct = default);
    void Update(RecurringTaskTemplate template);
    void Remove(RecurringTaskTemplate template);
    Task SaveChangesAsync(CancellationToken ct = default);
}
