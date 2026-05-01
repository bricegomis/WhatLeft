using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Tasks.Application.DTOs;

public record CreateRecurringTemplateRequest(
    string Title,
    double Duration = 1,
    string[]? Tags = null,
    RecurrenceType RecurrenceType = RecurrenceType.Weekly,
    int FrequencyPerPeriod = 1,
    int ResetHour = 21);
