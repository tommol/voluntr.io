using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;

/// <summary>
/// Command to add a new member to an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization.</param>
/// <param name="ActorUserId">The ID of the user performing the action (must have permission).</param>
/// <param name="UserId">The ID of the user to add as a member.</param>
/// <param name="Role">The role to assign to the new member.</param>
/// <param name="IdempotencyKey">Optional idempotency key for duplicate prevention.</param>
public sealed record AddOrganizationMemberCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid UserId,
    string Role,
    string? IdempotencyKey) : IIdempotentCommand<Result<AddOrganizationMemberResponse>>;
