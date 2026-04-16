namespace Voluntrio.Common.Domain.Abstractions;

public abstract record DomainEvent(DateTime OccurredOnUtc) : IDomainEvent;
