using WhatLeft.Tasks.Domain.Entities;

namespace WhatLeft.Tasks.Domain.Repositories;

public interface IRecurringTemplateRepository
{
    Task<IEnumerable<RecurringTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<RecurringTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RecurringTemplate template, CancellationToken ct = default);
    void Update(RecurringTemplate template);
    void Remove(RecurringTemplate template);
    Task SaveChangesAsync(CancellationToken ct = default);
}
