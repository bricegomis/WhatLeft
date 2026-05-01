namespace WhatLeft.Tasks.Application.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt,
    double Duration,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt,
    List<string> Tags,
    DateTimeOffset? CancelledAt,
    Guid? RecurringTemplateId);
