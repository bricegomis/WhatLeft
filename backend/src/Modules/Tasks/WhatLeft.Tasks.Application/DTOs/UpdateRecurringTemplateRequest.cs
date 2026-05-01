namespace WhatLeft.Tasks.Application.DTOs;

public record UpdateRecurringTemplateRequest(
    string? Title = null,
    double? Duration = null,
    List<string>? Tags = null);
