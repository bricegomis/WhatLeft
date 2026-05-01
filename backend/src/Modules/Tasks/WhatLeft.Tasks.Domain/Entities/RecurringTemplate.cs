using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Tasks.Domain.Entities;

/// <summary>
/// Template defining a recurring task pattern.
/// The background service uses this to generate TaskItem instances each period
/// and cancel the previous period's uncompleted ones.
/// </summary>
public sealed class RecurringTemplate
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public double Duration { get; private set; }
    public string? TagsRaw { get; private set; }
    public RecurrenceType RecurrenceType { get; private set; }

    /// <summary>Hour of day (UTC) at which the reset is triggered (0-23).</summary>
    public int ResetHour { get; private set; }

    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Required by EF Core
    private RecurringTemplate() { }

    public string[] Tags => TagsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

    public static RecurringTemplate Create(
        string title,
        double duration,
        string[] tags,
        RecurrenceType type,
        int resetHour = 21)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (duration <= 0) throw new ArgumentException("Duration must be positive.", nameof(duration));
        if (resetHour is < 0 or > 23) throw new ArgumentException("ResetHour must be 0-23.", nameof(resetHour));

        return new RecurringTemplate
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Duration = duration,
            TagsRaw = tags.Length > 0 ? string.Join(",", tags) : null,
            RecurrenceType = type,
            ResetHour = resetHour,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string? title, double? duration, string[]? tags, int? resetHour)
    {
        if (title is not null) Title = title.Trim();
        if (duration is > 0) Duration = duration.Value;
        if (tags is not null) TagsRaw = tags.Length > 0 ? string.Join(",", tags) : null;
        if (resetHour.HasValue && resetHour is >= 0 and <= 23) ResetHour = resetHour.Value;
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
