using System.Security.Claims;
using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Application.UseCases;

namespace WhatLeft.Api.Modules.Tasks;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks")
            .RequireAuthorization()
            .WithTags("Tasks");

        // GET /tasks
        group.MapGet("/", async (HttpContext ctx, TaskService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(UserId(ctx), ct)));

        // GET /tasks/history
        group.MapGet("/history", async (HttpContext ctx, TaskService service, CancellationToken ct) =>
            Results.Ok(await service.GetHistoryAsync(UserId(ctx), ct)));

        // POST /tasks
        group.MapPost("/", async (HttpContext ctx, CreateTaskRequest request, TaskService service, CancellationToken ct) =>
        {
            var task = await service.CreateAsync(request, UserId(ctx), ct);
            return Results.Created($"/tasks/{task.Id}", task);
        });

        // PUT /tasks/{id}
        group.MapPut("/{id:guid}", async (HttpContext ctx, Guid id, UpdateTaskRequest request, TaskService service, CancellationToken ct) =>
        {
            var task = await service.UpdateAsync(id, request, UserId(ctx), ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        // DELETE /tasks/{id}
        group.MapDelete("/{id:guid}", async (HttpContext ctx, Guid id, TaskService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, UserId(ctx), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        // POST /tasks/{id}/reactivate
        group.MapPost("/{id:guid}/reactivate", async (HttpContext ctx, Guid id, TaskService service, CancellationToken ct) =>
        {
            var task = await service.ReactivateAsync(id, UserId(ctx), ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        return app;
    }

    private static string UserId(HttpContext ctx) =>
        ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
}
