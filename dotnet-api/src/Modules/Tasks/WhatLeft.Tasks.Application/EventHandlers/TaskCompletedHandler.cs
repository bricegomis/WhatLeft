using MediatR;
using Microsoft.Extensions.Logging;
using WhatLeft.Tasks.Domain.Events;

namespace WhatLeft.Tasks.Application.EventHandlers;

/// <summary>
/// Handles TaskCompletedEvent raised by the Tasks domain.
///
/// This is the integration point for cross-domain reactions:
/// - Points module: award points for completing a task
/// - Automations module: trigger automation rules
/// - Habits module: track habit streaks
///
/// To add a new cross-domain reaction, create a NEW handler in the target module.
/// Never add direct calls between domain services here.
/// </summary>
public sealed class TaskCompletedHandler(ILogger<TaskCompletedHandler> logger)
    : INotificationHandler<TaskCompletedEvent>
{
    public Task Handle(TaskCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[Tasks] Task {TaskId} '{Title}' was completed at {CompletedAt}.",
            notification.TaskId,
            notification.Title,
            notification.CompletedAt);

        // Example: future Points module handler would look like:
        // await pointsService.AwardPointsAsync(userId, PointsReason.TaskCompleted, 10, ct);

        return Task.CompletedTask;
    }
}
