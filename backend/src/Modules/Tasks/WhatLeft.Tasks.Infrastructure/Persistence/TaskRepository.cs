using Microsoft.EntityFrameworkCore;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Infrastructure.Persistence;

public sealed class TaskRepository(TasksDbContext context) : ITaskRepository
{
    public async Task<IEnumerable<TaskItem>> GetAllAsync(string userId, CancellationToken ct = default) =>
        await context.Tasks
            .Where(t => t.UserId == userId && t.FinishAt == null && t.CancelledAt == null)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<TaskItem>> GetHistoryAsync(string userId, CancellationToken ct = default) =>
        await context.Tasks
            .Where(t => t.UserId == userId && (t.FinishAt != null || t.CancelledAt != null))
            .OrderByDescending(t => t.CancelledAt ?? t.FinishAt)
            .ToListAsync(ct);

    public Task<int> CountByTemplateAndPeriodAsync(Guid templateId, DateTimeOffset periodStart, CancellationToken ct = default) =>
        context.Tasks.CountAsync(t => t.RecurringTaskTemplateId == templateId && t.PeriodStart == periodStart, ct);

    public async Task<IEnumerable<TaskItem>> GetUnfinishedByTemplateAndPeriodAsync(Guid templateId, DateTimeOffset periodStart, CancellationToken ct = default) =>
        await context.Tasks
            .Where(t => t.RecurringTaskTemplateId == templateId
                     && t.PeriodStart == periodStart
                     && t.FinishAt == null
                     && t.CancelledAt == null)
            .ToListAsync(ct);

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<TaskItem?> GetByIdForUserAsync(Guid id, string userId, CancellationToken ct = default) =>
        context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    public async Task AddAsync(TaskItem task, CancellationToken ct = default) =>
        await context.Tasks.AddAsync(task, ct);

    public void Update(TaskItem task) =>
        context.Tasks.Update(task);

    public void Remove(TaskItem task) =>
        context.Tasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
