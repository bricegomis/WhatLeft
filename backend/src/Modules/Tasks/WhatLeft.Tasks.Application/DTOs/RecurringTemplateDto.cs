using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Tasks.Application.DTOs;

public record RecurringTemplateDto(
    Guid Id,
    string Title,
    double Duration,
    string[] Tags,
    RecurrenceType RecurrenceType,
    int FrequencyPerPeriod,
    int ResetHour,
    bool IsActive,
    DateTimeOffset CreatedAt);
