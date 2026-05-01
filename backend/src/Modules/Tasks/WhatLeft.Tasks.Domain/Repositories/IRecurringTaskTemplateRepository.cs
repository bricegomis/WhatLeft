using WhatLeft.Tasks.Domain.Entities;

namespace WhatLeft.Tasks.Domain.Repositories;

public interface IRecurringTaskTemplateRepository
{
    Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<RecurringTaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RecurringTaskTemplate template, CancellationToken ct = default);
    void Update(RecurringTaskTemplate template);
    void Remove(RecurringTaskTemplate template);
    Task SaveChangesAsync(CancellationToken ct = default);
}
