namespace WhatLeft.Tasks.Domain.Primitives;

/// <summary>
/// Base class for aggregates. Collects domain events raised during business operations.
/// Events are dispatched by the Application layer after persistence (not before).
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
