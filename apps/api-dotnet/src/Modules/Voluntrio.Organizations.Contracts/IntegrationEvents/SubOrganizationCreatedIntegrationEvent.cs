using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Organizations.Contracts.IntegrationEvents;

public sealed record SubOrganizationCreatedIntegrationEvent(
    Guid MessageId,
    DateTime OccurredOnUtc,
    Guid OrganizationId,
    Guid ParentOrganizationId,
    Guid OwnerUserId) : IIntegrationEvent;
