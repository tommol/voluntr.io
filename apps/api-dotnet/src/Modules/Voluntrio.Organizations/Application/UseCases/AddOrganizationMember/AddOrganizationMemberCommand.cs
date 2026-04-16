using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;

public sealed record AddOrganizationMemberCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid UserId,
    string Role,
    string? IdempotencyKey) : IIdempotentCommand<Result<AddOrganizationMemberResponse>>;
