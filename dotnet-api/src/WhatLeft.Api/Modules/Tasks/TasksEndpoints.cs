using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Application.UseCases;

namespace WhatLeft.Api.Modules.Tasks;

/// <summary>
/// Registers all Tasks REST endpoints on the app.
/// Each module exposes a MapXxxEndpoints() extension — Program.cs just calls them all.
/// Adding a new module = adding one line in Program.cs.
/// </summary>
public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks")
            .RequireAuthorization()
            .WithTags("Tasks");

        // GET /tasks
        group.MapGet("/", async (TaskService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        // POST /tasks
        group.MapPost("/", async (CreateTaskRequest request, TaskService service, CancellationToken ct) =>
        {
            var task = await service.CreateAsync(request, ct);
            return Results.Created($"/tasks/{task.Id}", task);
        });

        // PUT /tasks/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest request, TaskService service, CancellationToken ct) =>
        {
            var task = await service.UpdateAsync(id, request, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        // DELETE /tasks/{id}
        group.MapDelete("/{id:guid}", async (Guid id, TaskService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
