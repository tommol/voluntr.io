using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.CreateSubOrganization;

namespace Voluntrio.Organizations.Application.UseCases.CreateSubOrganization;

/// <summary>
/// Command to create a new sub-organization under a parent organization.
/// </summary>
/// <param name="ParentOrganizationId">The ID of the parent organization.</param>
/// <param name="Name">The name of the sub-organization to create.</param>
/// <param name="ActorUserId">The user ID performing the operation.</param>
/// <param name="IdempotencyKey">Optional idempotency key for duplicate prevention.</param>
public sealed record CreateSubOrganizationCommand(
    Guid ParentOrganizationId,
    string Name,
    Guid ActorUserId,
    string? IdempotencyKey) : IIdempotentCommand<Result<CreateSubOrganizationResponse>>;
