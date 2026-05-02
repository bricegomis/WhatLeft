using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Tasks.Domain.Entities;

/// <summary>
/// Template defining a recurring task pattern.
/// The background service uses this to generate TaskItem instances each period
/// and cancel the previous period's uncompleted ones.
/// </summary>
public sealed class RecurringTaskTemplate
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public double Duration { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public RecurrenceType RecurrenceType { get; private set; }

    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Required by EF Core
    private RecurringTaskTemplate() { }

    public static RecurringTaskTemplate Create(
        string userId,
        string title,
        double duration,
        List<string> tags,
        RecurrenceType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (duration <= 0) throw new ArgumentException("Duration must be positive.", nameof(duration));

        return new RecurringTaskTemplate
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Duration = duration,
            Tags = tags.ToList(),
            RecurrenceType = type,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string? title, double? duration, List<string>? tags)
    {
        if (title is not null) Title = title.Trim();
        if (duration is > 0) Duration = duration.Value;
        if (tags is not null) Tags = tags.ToList();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    /// <summary>Returns the start of the current period (UTC) for this template.</summary>
    public DateTimeOffset GetCurrentPeriodStart(DateTimeOffset now)
    {
        return RecurrenceType switch
        {
            RecurrenceType.Daily => new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero),
            RecurrenceType.Weekly => GetWeekStart(now),
            _ => throw new InvalidOperationException($"Unknown recurrence type: {RecurrenceType}")
        };
    }

    /// <summary>Returns Monday 00:00 UTC of the week containing the given date.</summary>
    private static DateTimeOffset GetWeekStart(DateTimeOffset date)
    {
        var d = date.UtcDateTime.Date;
        int diff = (7 + ((int)d.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return new DateTimeOffset(d.AddDays(-diff), TimeSpan.Zero);
    }
}
