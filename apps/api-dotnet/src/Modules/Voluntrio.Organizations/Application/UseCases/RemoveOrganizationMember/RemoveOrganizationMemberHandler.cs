using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;

public sealed class RemoveOrganizationMemberHandler(
    AppDbContext dbContext,
    IClock clock) : ICommandHandler<RemoveOrganizationMemberCommand, Result<RemoveOrganizationMemberResponse>>
{
    public async Task<Result<RemoveOrganizationMemberResponse>> Handle(RemoveOrganizationMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<RemoveOrganizationMemberResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        try
        {
            organization.RemoveMember(command.ActorUserId, command.UserId, clock.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            var member = organization.Members.Single(x => x.UserId == command.UserId);
            return Result<RemoveOrganizationMemberResponse>.Success(
                new RemoveOrganizationMemberResponse(
                    organization.Id,
                    member.UserId,
                    member.Status.Value,
                    member.LeftAtUtc ?? clock.UtcNow));
        }
        catch (DomainException ex)
        {
            return Result<RemoveOrganizationMemberResponse>.Failure(new Error("organizations.members.remove_invalid", ex.Message));
        }
    }
}
