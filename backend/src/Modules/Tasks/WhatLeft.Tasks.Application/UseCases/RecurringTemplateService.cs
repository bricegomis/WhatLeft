using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Application.UseCases;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Enums;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Application.UseCases;

/// <summary>CRUD for recurring task templates.</summary>
public sealed class RecurringTaskTemplateService(
    IRecurringTaskTemplateRepository repository,
    RecurringTaskProcessor processor)
{
    public async Task<IEnumerable<RecurringTaskTemplateDto>> GetAllAsync(string userId, CancellationToken ct = default)
    {
        var templates = await repository.GetAllForUserAsync(userId, ct);
        return templates.Select(ToDto);
    }

    public async Task<RecurringTaskTemplateDto> CreateAsync(CreateRecurringTaskTemplateRequest request, string userId, CancellationToken ct = default)
    {
        var template = RecurringTaskTemplate.Create(
            userId,
            request.Title,
            request.Duration,
            request.Tags ?? [],
            request.RecurrenceType);

        await repository.AddAsync(template, ct);
        await repository.SaveChangesAsync(ct);
        return ToDto(template);
    }

    public async Task<RecurringTaskTemplateDto?> UpdateAsync(Guid id, UpdateRecurringTaskTemplateRequest request, string userId, CancellationToken ct = default)
    {
        var template = await repository.GetByIdForUserAsync(id, userId, ct);
        if (template is null) return null;

        template.Update(request.Title, request.Duration, request.Tags);
        repository.Update(template);
        await repository.SaveChangesAsync(ct);
        return ToDto(template);
    }

    public async Task<bool> DeleteAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var template = await repository.GetByIdForUserAsync(id, userId, ct);
        if (template is null) return false;

        template.Deactivate();
        repository.Update(template);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var template = await repository.GetByIdForUserAsync(id, userId, ct);
        if (template is null) return false;

        template.Activate();
        repository.Update(template);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Manually triggers processing for a specific template (ignores reset hour).</summary>
    public Task<bool> ProcessNowAsync(Guid id, CancellationToken ct = default) =>
        processor.ProcessTemplateAsync(id, ct);

    /// <summary>Cancels current period tasks and creates one for the next period.</summary>
    public Task<bool> AdvanceAsync(Guid id, CancellationToken ct = default) =>
        processor.AdvanceAsync(id, ct);

    /// <summary>Advances all active templates of a given recurrence type to the next period.</summary>
    public Task AdvanceAllByTypeAsync(RecurrenceType type, string userId, CancellationToken ct = default) =>
        processor.AdvanceAllByTypeAsync(type, userId, ct);

    private static RecurringTaskTemplateDto ToDto(RecurringTaskTemplate t) =>
        new(t.Id, t.Title, t.Duration, t.Tags, t.RecurrenceType, t.IsActive, t.CreatedAt);
}
