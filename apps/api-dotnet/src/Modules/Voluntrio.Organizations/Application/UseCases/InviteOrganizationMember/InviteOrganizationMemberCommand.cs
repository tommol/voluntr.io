using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.InviteOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.InviteOrganizationMember;

public sealed record InviteOrganizationMemberCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    string Email,
    string? GivenName,
    string? FamilyName,
    string Role,
    string? IdempotencyKey) : IIdempotentCommand<Result<InviteOrganizationMemberResponse>>;
