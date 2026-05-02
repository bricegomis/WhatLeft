using System.Security.Claims;
using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Application.UseCases;
using WhatLeft.Tasks.Domain.Enums;

namespace WhatLeft.Api.Modules.Tasks;

public static class RecurringTaskTemplatesEndpoints
{
    public static IEndpointRouteBuilder MapRecurringTaskTemplatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recurring-templates")
            .RequireAuthorization()
            .WithTags("RecurringTaskTemplates");

        // GET /recurring-templates
        group.MapGet("/", async (HttpContext ctx, RecurringTaskTemplateService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(UserId(ctx), ct)));

        // POST /recurring-templates
        group.MapPost("/", async (HttpContext ctx, CreateRecurringTaskTemplateRequest request, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var template = await service.CreateAsync(request, UserId(ctx), ct);
            return Results.Created($"/recurring-templates/{template.Id}", template);
        });

        // PUT /recurring-templates/{id}
        group.MapPut("/{id:guid}", async (HttpContext ctx, Guid id, UpdateRecurringTaskTemplateRequest request, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var template = await service.UpdateAsync(id, request, UserId(ctx), ct);
            return template is null ? Results.NotFound() : Results.Ok(template);
        });

        // DELETE /recurring-templates/{id} — soft deactivate
        group.MapDelete("/{id:guid}", async (HttpContext ctx, Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, UserId(ctx), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        // POST /recurring-templates/{id}/activate
        group.MapPost("/{id:guid}/activate", async (HttpContext ctx, Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var ok = await service.ActivateAsync(id, UserId(ctx), ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        // POST /recurring-templates/{id}/process
        group.MapPost("/{id:guid}/process", async (Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var ok = await service.ProcessNowAsync(id, ct);
            return ok ? Results.Ok() : Results.NotFound();
        });

        // POST /recurring-templates/advance-all?type=Daily|Weekly
        group.MapPost("/advance-all", async (HttpContext ctx, RecurrenceType type, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            await service.AdvanceAllByTypeAsync(type, UserId(ctx), ct);
            return Results.Ok();
        });

        // POST /recurring-templates/{id}/advance
        group.MapPost("/{id:guid}/advance", async (Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var ok = await service.AdvanceAsync(id, ct);
            return ok ? Results.Ok() : Results.NotFound();
        });

        return app;
    }

    private static string UserId(HttpContext ctx) =>
        ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
}

