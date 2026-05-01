using WhatLeft.Tasks.Application.DTOs;
using WhatLeft.Tasks.Application.UseCases;
using WhatLeft.Tasks.Domain.Entities;
using WhatLeft.Tasks.Domain.Repositories;

namespace WhatLeft.Tasks.Application.UseCases;

/// <summary>CRUD for recurring task templates.</summary>
public sealed class RecurringTaskTemplateService(
    IRecurringTaskTemplateRepository repository,
    RecurringTaskProcessor processor)
{
    public async Task<IEnumerable<RecurringTaskTemplateDto>> GetAllAsync(CancellationToken ct = default)
    {
        var templates = await repository.GetAllAsync(ct);
        return templates.Select(ToDto);
    }

    public async Task<RecurringTaskTemplateDto> CreateAsync(CreateRecurringTaskTemplateRequest request, CancellationToken ct = default)
    {
        var template = RecurringTaskTemplate.Create(
            request.Title,
            request.Duration,
            request.Tags ?? [],
            request.RecurrenceType);

        await repository.AddAsync(template, ct);
        await repository.SaveChangesAsync(ct);
        return ToDto(template);
    }

    public async Task<RecurringTaskTemplateDto?> UpdateAsync(Guid id, UpdateRecurringTaskTemplateRequest request, CancellationToken ct = default)
    {
        var template = await repository.GetByIdAsync(id, ct);
        if (template is null) return null;

        template.Update(request.Title, request.Duration, request.Tags);
        repository.Update(template);
        await repository.SaveChangesAsync(ct);
        return ToDto(template);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await repository.GetByIdAsync(id, ct);
        if (template is null) return false;

        template.Deactivate();
        repository.Update(template);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var template = await repository.GetByIdAsync(id, ct);
        if (template is null) return false;

        template.Activate();
        repository.Update(template);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Manually triggers processing for a specific template (ignores reset hour).</summary>
    public Task<bool> ProcessNowAsync(Guid id, CancellationToken ct = default) =>
        processor.ProcessTemplateAsync(id, ct);

    private static RecurringTaskTemplateDto ToDto(RecurringTaskTemplate t) =>
        new(t.Id, t.Title, t.Duration, t.Tags, t.RecurrenceType, t.IsActive, t.CreatedAt);
}
