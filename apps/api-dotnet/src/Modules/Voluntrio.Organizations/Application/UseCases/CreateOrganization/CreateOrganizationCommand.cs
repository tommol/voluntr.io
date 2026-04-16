using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;

namespace Voluntrio.Organizations.Application.UseCases.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string Name,
    Guid OwnerUserId,
    string? IdempotencyKey) : IIdempotentCommand<Result<CreateOrganizationResponse>>;
