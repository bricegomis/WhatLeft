using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatLeft.Tasks.Application.UseCases;

namespace WhatLeft.Tasks.Infrastructure.BackgroundServices;

/// <summary>
/// Checks every hour whether any recurring template's reset time has passed,
/// and if so: cancels the previous period's uncompleted tasks and generates new ones.
/// Uses IServiceScopeFactory because the processor and repositories are Scoped.
/// </summary>
public sealed class RecurringTaskBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringTaskBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RecurringTaskBackgroundService started.");

        // Run once immediately on startup, then every hour
        await RunAsync(stoppingToken);

        using var timer = new PeriodicTimer(CheckInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunAsync(stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<RecurringTaskProcessor>();
            await processor.ProcessAllAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error during recurring task processing.");
        }
    }
}
