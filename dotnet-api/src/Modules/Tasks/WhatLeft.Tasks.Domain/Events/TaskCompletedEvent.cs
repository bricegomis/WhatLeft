using WhatLeft.Tasks.Domain.Primitives;

namespace WhatLeft.Tasks.Domain.Events;

/// <summary>
/// Raised when a task's FinishAt is set for the first time.
/// Other modules (Points, Automations) can react to this event without coupling.
/// </summary>
public record TaskCompletedEvent(
    Guid TaskId,
    string Title,
    DateTimeOffset CompletedAt) : IDomainEvent;
