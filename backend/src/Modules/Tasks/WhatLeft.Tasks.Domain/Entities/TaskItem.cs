using WhatLeft.Tasks.Domain.Events;
using WhatLeft.Tasks.Domain.Primitives;

namespace WhatLeft.Tasks.Domain.Entities;

/// <summary>
/// Task aggregate root.
/// Named TaskItem to avoid conflict with System.Threading.Tasks.Task.
/// Business rules are enforced here — never in the Application layer.
/// </summary>
public sealed class TaskItem : AggregateRoot
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public double Duration { get; private set; }
    public DateTimeOffset? StartAt { get; private set; }
    public DateTimeOffset? FinishAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public List<string> Tags { get; private set; } = [];

    /// <summary>Links this instance to the recurring template that generated it.</summary>
    public Guid? RecurringTaskTemplateId { get; private set; }

    /// <summary>Start of the period (week or day) this instance belongs to.</summary>
    public DateTimeOffset? PeriodStart { get; private set; }

    // Required by EF Core
    private TaskItem() { }

    /// <summary>Factory method — the only valid way to create a manual task.</summary>
    public static TaskItem Create(
        string userId,
        string title,
        double duration,
        DateTimeOffset? startAt,
        List<string>? tags)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (duration <= 0) throw new ArgumentException("Duration must be positive.", nameof(duration));

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            Duration = duration,
            StartAt = startAt,
            Tags = tags ?? []
        };
    }

    /// <summary>Creates a task instance generated from a recurring template.</summary>
    public static TaskItem CreateFromTemplate(RecurringTaskTemplate template, DateTimeOffset periodStart) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = template.UserId,
            Title = template.Title,
            CreatedAt = DateTimeOffset.UtcNow,
            Duration = template.Duration,
            Tags = [.. template.Tags],
            RecurringTaskTemplateId = template.Id,
            PeriodStart = periodStart
        };

    /// <summary>Marks this task as cancelled (non-completed at period reset).</summary>
    public void Cancel()
    {
        if (CancelledAt is null)
            CancelledAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Reactivates a finished or cancelled task by clearing completion/cancellation flags.</summary>
    public void Reactivate()
    {
        FinishAt = null;
        CancelledAt = null;
    }

    /// <summary>
    /// Updates allowed fields. Raises TaskCompletedEvent when FinishAt is set for the first time.
    /// This is the single place in the codebase where TaskCompletedEvent is raised.
    /// </summary>
    public void Update(
        string? title,
        double? duration,
        DateTimeOffset? startAt,
        DateTimeOffset? finishAt,
        List<string>? tags)
    {
        if (title is not null) Title = title.Trim();
        if (duration is > 0) Duration = duration.Value;
        if (startAt.HasValue) StartAt = startAt;
        if (tags is not null) Tags = tags;

        // ← Domain event raised here when task transitions to completed
        if (finishAt.HasValue && FinishAt is null)
        {
            FinishAt = finishAt.Value;
            RaiseDomainEvent(new TaskCompletedEvent(Id, Title, finishAt.Value));
        }
        else if (finishAt.HasValue)
        {
            FinishAt = finishAt.Value;
        }
    }
}
