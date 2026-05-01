using Microsoft.EntityFrameworkCore;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Infrastructure.Persistence;

public sealed class RecurringTemplateRepository(TasksDbContext context) : IRecurringTemplateRepository
{
    public async Task<IEnumerable<RecurringTemplate>> GetAllAsync(CancellationToken ct = default) =>
        await context.RecurringTemplates
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public Task<RecurringTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.RecurringTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(RecurringTemplate template, CancellationToken ct = default) =>
        await context.RecurringTemplates.AddAsync(template, ct);

    public void Update(RecurringTemplate template) =>
        context.RecurringTemplates.Update(template);

    public void Remove(RecurringTemplate template) =>
        context.RecurringTemplates.Remove(template);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
