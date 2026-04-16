using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;

/// <summary>
/// Command to remove a member from an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization.</param>
/// <param name="ActorUserId">The ID of the user performing the action (must have permission).</param>
/// <param name="UserId">The ID of the user to remove from the organization.</param>
/// <param name="IdempotencyKey">Optional idempotency key for duplicate prevention.</param>
public sealed record RemoveOrganizationMemberCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid UserId,
    string? IdempotencyKey) : IIdempotentCommand<Result<RemoveOrganizationMemberResponse>>;
