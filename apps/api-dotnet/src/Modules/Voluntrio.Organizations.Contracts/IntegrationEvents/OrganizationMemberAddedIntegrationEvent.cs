using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Organizations.Contracts.IntegrationEvents;

public sealed record OrganizationMemberAddedIntegrationEvent(
    Guid MessageId,
    DateTime OccurredOnUtc,
    Guid OrganizationId,
    Guid UserId) : IIntegrationEvent;
