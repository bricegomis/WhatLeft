namespace WhatLeft.Tasks.Application.DTOs;

public record UpdateTaskRequest(
    string? Title = null,
    double? Duration = null,
    DateTimeOffset? StartAt = null,
    DateTimeOffset? FinishAt = null,
    List<string>? Tags = null);
