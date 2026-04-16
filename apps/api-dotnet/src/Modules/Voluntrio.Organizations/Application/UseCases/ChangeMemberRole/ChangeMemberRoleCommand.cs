using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;

namespace Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;

public sealed record ChangeMemberRoleCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid MemberUserId,
    string Role,
    string? IdempotencyKey) : IIdempotentCommand<Result<ChangeMemberRoleResponse>>;
