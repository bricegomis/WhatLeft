namespace WhatLeft.Tasks.Application.DTOs;

public record UpdateRecurringTaskTemplateRequest(
    string? Title = null,
    double? Duration = null,
    List<string>? Tags = null);
