using Microsoft.EntityFrameworkCore;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Infrastructure.Persistence;

public sealed class RecurringTaskTemplateRepository(TasksDbContext context) : IRecurringTaskTemplateRepository
{
    public async Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync(CancellationToken ct = default) =>
        await context.RecurringTaskTemplates
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public Task<RecurringTaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.RecurringTaskTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(RecurringTaskTemplate template, CancellationToken ct = default) =>
        await context.RecurringTaskTemplates.AddAsync(template, ct);

    public void Update(RecurringTaskTemplate template) =>
        context.RecurringTaskTemplates.Update(template);

    public void Remove(RecurringTaskTemplate template) =>
        context.RecurringTaskTemplates.Remove(template);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
