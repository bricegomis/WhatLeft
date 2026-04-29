namespace WhatLeft.Tasks.Application.DTOs;

public record CreateTaskRequest(
    string Title,
    double Duration = 1,
    DateTimeOffset? StartAt = null,
    List<string>? Tags = null);
