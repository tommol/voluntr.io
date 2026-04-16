using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Organizations.Contracts.IntegrationEvents;

public sealed record OrganizationMemberRemovedIntegrationEvent(
    Guid MessageId,
    DateTime OccurredOnUtc,
    Guid OrganizationId,
    Guid UserId) : IIntegrationEvent;
