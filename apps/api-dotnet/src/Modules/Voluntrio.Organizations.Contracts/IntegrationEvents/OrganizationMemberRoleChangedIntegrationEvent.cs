using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Organizations.Contracts.IntegrationEvents;

public sealed record OrganizationMemberRoleChangedIntegrationEvent(
    Guid MessageId,
    DateTime OccurredOnUtc,
    Guid OrganizationId,
    Guid UserId,
    string Role) : IIntegrationEvent;
