using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;

namespace Voluntrio.Organizations.Application.UseCases.CreateOrganization;

/// <summary>
/// Command to create a new organization.
/// </summary>
/// <param name="Name">The name of the organization to create.</param>
/// <param name="OwnerUserId">The user ID of the organization owner.</param>
/// <param name="IdempotencyKey">Optional idempotency key for duplicate prevention.</param>
public sealed record CreateOrganizationCommand(
    string Name,
    Guid OwnerUserId,
    string? IdempotencyKey) : IIdempotentCommand<Result<CreateOrganizationResponse>>;
