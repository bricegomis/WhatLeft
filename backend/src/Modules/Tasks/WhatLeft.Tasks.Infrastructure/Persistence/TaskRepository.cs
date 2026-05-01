using Microsoft.EntityFrameworkCore;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Infrastructure.Persistence;

public sealed class TaskRepository(TasksDbContext context) : ITaskRepository
{
    public async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken ct = default) =>
        await context.Tasks
            .Where(t => t.FinishAt == null)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(TaskItem task, CancellationToken ct = default) =>
        await context.Tasks.AddAsync(task, ct);

    public void Update(TaskItem task) =>
        context.Tasks.Update(task);

    public void Remove(TaskItem task) =>
        context.Tasks.Remove(task);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
