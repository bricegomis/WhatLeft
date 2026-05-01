using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Tasks.Application.DTOs;

public record CreateRecurringTemplateRequest(
    string Title,
    double Duration = 30,
    List<string>? Tags = null,
    RecurrenceType RecurrenceType = RecurrenceType.Weekly);
