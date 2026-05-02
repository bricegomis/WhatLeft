using Microsoft.Extensions.Logging;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Enums;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Application.UseCases;

/// <summary>
/// Core processing logic for recurring tasks.
/// Scoped service — used by the background service (via IServiceScopeFactory)
/// and directly by the manual-trigger endpoint.
/// </summary>
public sealed class RecurringTaskProcessor(
    IRecurringTaskTemplateRepository templateRepo,
    ITaskRepository taskRepo,
    ILogger<RecurringTaskProcessor> logger)
{
    /// <summary>
    /// Processes all active templates whose reset hour has passed for the current period.
    /// Idempotent: skips templates that already have instances for the current period.
    /// </summary>
    public async Task ProcessAllAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var templates = await templateRepo.GetAllAsync(ct);

        foreach (var template in templates.Where(t => t.IsActive))
        {
            var periodStart = template.GetCurrentPeriodStart(now);

            await ProcessSingleAsync(template, periodStart, ct);
        }
    }

    /// <summary>Forces processing of a specific template, regardless of reset hour.</summary>
    public async Task<bool> ProcessTemplateAsync(Guid templateId, CancellationToken ct = default)
    {
        var template = await templateRepo.GetByIdAsync(templateId, ct);
        if (template is null || !template.IsActive) return false;

        var periodStart = template.GetCurrentPeriodStart(DateTimeOffset.UtcNow);
        await ProcessSingleAsync(template, periodStart, ct);
        return true;
    }

    /// <summary>
    /// Cancels current period tasks and creates next period task for ALL active templates of a given type for a user.
    /// </summary>
    public async Task AdvanceAllByTypeAsync(RecurrenceType type, string userId, CancellationToken ct = default)
    {
        var templates = await templateRepo.GetAllAsync(ct);
        foreach (var template in templates.Where(t => t.IsActive && t.RecurrenceType == type && t.UserId == userId))
            await AdvanceSingleAsync(template, ct);
    }

    /// <summary>
    /// Cancels unfinished tasks for the current period and creates a task for the next period.
    /// Useful when you want to skip to the next cycle early.
    /// </summary>
    public async Task<bool> AdvanceAsync(Guid templateId, CancellationToken ct = default)
    {
        var template = await templateRepo.GetByIdAsync(templateId, ct);
        if (template is null || !template.IsActive) return false;
        await AdvanceSingleAsync(template, ct);
        return true;
    }

    private async Task AdvanceSingleAsync(RecurringTaskTemplate template, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var currentPeriodStart = template.GetCurrentPeriodStart(now);

        // Cancel unfinished tasks for the current period
        var currentTasks = await taskRepo.GetUnfinishedByTemplateAndPeriodAsync(template.Id, currentPeriodStart, ct);
        foreach (var task in currentTasks)
        {
            task.Cancel();
            taskRepo.Update(task);
        }

        // Compute next period start
        var nextPeriodStart = template.RecurrenceType == RecurrenceType.Daily
            ? currentPeriodStart.AddDays(1)
            : currentPeriodStart.AddDays(7);

        // Create one task for the next period
        var newTask = TaskItem.CreateFromTemplate(template, nextPeriodStart);
        await taskRepo.AddAsync(newTask, ct);

        await taskRepo.SaveChangesAsync(ct);

        logger.LogInformation(
            "Recurring advance: template {Id} ({Title}), next period {Period}",
            template.Id, template.Title, nextPeriodStart);
    }

    private async Task ProcessSingleAsync(RecurringTaskTemplate template, DateTimeOffset periodStart, CancellationToken ct)
    {
        // Idempotency guard: skip if instances already exist for this period
        var existingCount = await taskRepo.CountByTemplateAndPeriodAsync(template.Id, periodStart, ct);
        if (existingCount > 0) return;

        logger.LogInformation(
            "Recurring: processing template {Id} ({Title}), period {Period}",
            template.Id, template.Title, periodStart);

        // Cancel previous period's uncompleted instances
        var previousPeriodStart = template.RecurrenceType == RecurrenceType.Daily
            ? periodStart.AddDays(-1)
            : periodStart.AddDays(-7);

        var unfinished = await taskRepo.GetUnfinishedByTemplateAndPeriodAsync(template.Id, previousPeriodStart, ct);
        foreach (var task in unfinished)
        {
            task.Cancel();
            taskRepo.Update(task);
        }

        // Create one new task instance for the current period
        var newTask = TaskItem.CreateFromTemplate(template, periodStart);
        await taskRepo.AddAsync(newTask, ct);

        await taskRepo.SaveChangesAsync(ct);
    }
}
