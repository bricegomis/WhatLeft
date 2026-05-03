using WhatLeft.Tasks.Application.UseCases;

namespace WhatLeft.Tasks.Infrastructure.Jobs;

/// <summary>
/// Hangfire job that processes all active recurring templates.
/// Scheduled once per day at midnight UTC (replaces RecurringTaskBackgroundService).
/// Idempotent: each template checks whether instances already exist for the current period.
/// </summary>
public sealed class RecurringTaskJob(RecurringTaskProcessor processor)
{
    public const string JobId = "recurring-tasks-midnight";

    public async Task ProcessAsync() => await processor.ProcessAllAsync();
}
