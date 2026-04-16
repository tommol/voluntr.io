using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;

public sealed record RemoveOrganizationMemberCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid UserId,
    string? IdempotencyKey) : IIdempotentCommand<Result<RemoveOrganizationMemberResponse>>;
