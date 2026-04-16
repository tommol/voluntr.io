using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;

namespace Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;

/// <summary>
/// Command to change a member's role within an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization.</param>
/// <param name="ActorUserId">The ID of the user performing the action (must have permission).</param>
/// <param name="MemberUserId">The ID of the member whose role will be changed.</param>
/// <param name="Role">The new role to assign to the member.</param>
/// <param name="IdempotencyKey">Optional idempotency key for duplicate prevention.</param>
public sealed record ChangeMemberRoleCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid MemberUserId,
    string Role,
    string? IdempotencyKey) : IIdempotentCommand<Result<ChangeMemberRoleResponse>>;
