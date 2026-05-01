namespace WhatLeft.Tasks.Application.DTOs;

public record UpdateRecurringTemplateRequest(
    string? Title = null,
    double? Duration = null,
    string[]? Tags = null,
    int? ResetHour = null);
