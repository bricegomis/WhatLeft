using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Application.UseCases;

namespace WhatLeft.Api.Modules.Tasks;

public static class RecurringTaskTemplatesEndpoints
{
    public static IEndpointRouteBuilder MapRecurringTaskTemplatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recurring-templates")
            .RequireAuthorization()
            .WithTags("RecurringTaskTemplates");

        // GET /recurring-templates
        group.MapGet("/", async (RecurringTaskTemplateService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        // POST /recurring-templates
        group.MapPost("/", async (CreateRecurringTaskTemplateRequest request, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var template = await service.CreateAsync(request, ct);
            return Results.Created($"/recurring-templates/{template.Id}", template);
        });

        // PUT /recurring-templates/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateRecurringTaskTemplateRequest request, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var template = await service.UpdateAsync(id, request, ct);
            return template is null ? Results.NotFound() : Results.Ok(template);
        });

        // DELETE /recurring-templates/{id} — soft deactivate
        group.MapDelete("/{id:guid}", async (Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        // POST /recurring-templates/{id}/activate
        group.MapPost("/{id:guid}/activate", async (Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var ok = await service.ActivateAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        // POST /recurring-templates/{id}/process — manual trigger (generates tasks now)
        group.MapPost("/{id:guid}/process", async (Guid id, RecurringTaskTemplateService service, CancellationToken ct) =>
        {
            var ok = await service.ProcessNowAsync(id, ct);
            return ok ? Results.Ok() : Results.NotFound();
        });

        return app;
    }
}
