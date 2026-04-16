using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Organizations.Contracts.IntegrationEvents;

public sealed record OrganizationCreatedIntegrationEvent(
    Guid MessageId,
    DateTime OccurredOnUtc,
    Guid OrganizationId,
    Guid OwnerUserId) : IIntegrationEvent;
