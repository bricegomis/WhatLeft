using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Tasks.Application.DTOs;

public record RecurringTemplateDto(
    Guid Id,
    string Title,
    double Duration,
    List<string> Tags,
    RecurrenceType RecurrenceType,
    bool IsActive,
    DateTimeOffset CreatedAt);
