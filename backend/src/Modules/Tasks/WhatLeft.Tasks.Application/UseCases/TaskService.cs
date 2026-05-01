using MediatR;
using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Application.UseCases;

/// <summary>
/// Application service for the Tasks domain.
/// Orchestrates use cases: validates input, coordinates domain + repository,
/// then dispatches domain events collected by the aggregate.
/// Does NOT contain business logic — that lives in TaskItem.
/// </summary>
public sealed class TaskService(ITaskRepository repository, IPublisher publisher)
{
    public async Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tasks = await repository.GetAllAsync(ct);
        return tasks.Select(ToDto);
    }

    public async Task<IEnumerable<TaskDto>> GetHistoryAsync(CancellationToken ct = default)
    {
        var tasks = await repository.GetHistoryAsync(ct);
        return tasks.Select(ToDto);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        var task = TaskItem.Create(request.Title, request.Duration, request.StartAt, request.Tags);

        await repository.AddAsync(task, ct);
        await repository.SaveChangesAsync(ct);

        return ToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await repository.GetByIdAsync(id, ct);
        if (task is null) return null;

        task.Update(request.Title, request.Duration, request.StartAt, request.FinishAt, request.Tags);

        repository.Update(task);
        await repository.SaveChangesAsync(ct);

        // ← Domain events dispatched AFTER persistence (never before)
        // MediatR delivers them in-process to all registered INotificationHandlers
        foreach (var domainEvent in task.DomainEvents)
            await publisher.Publish(domainEvent, ct);

        task.ClearDomainEvents();

        return ToDto(task);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await repository.GetByIdAsync(id, ct);
        if (task is null) return false;

        repository.Remove(task);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TaskDto?> ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var task = await repository.GetByIdAsync(id, ct);
        if (task is null) return null;

        task.Reactivate();
        repository.Update(task);
        await repository.SaveChangesAsync(ct);
        return ToDto(task);
    }

    private static TaskDto ToDto(TaskItem t) =>
        new(t.Id, t.Title, t.CreatedAt, t.Duration, t.StartAt, t.FinishAt, t.Tags, t.CancelledAt, t.RecurringTaskTemplateId);
}
