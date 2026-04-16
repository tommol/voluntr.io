using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;

public sealed class ChangeMemberRoleHandler(
    AppDbContext dbContext,
    IClock clock) : ICommandHandler<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>
{
    public async Task<Result<ChangeMemberRoleResponse>> Handle(ChangeMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var organization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        try
        {
            organization.ChangeMemberRole(command.ActorUserId, command.MemberUserId, new MembershipRole(command.Role), clock.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            var membership = organization.Members.Single(x => x.UserId == command.MemberUserId);

            return Result<ChangeMemberRoleResponse>.Success(
                new ChangeMemberRoleResponse(organization.Id, membership.ToDto()));
        }
        catch (DomainException ex)
        {
            return Result<ChangeMemberRoleResponse>.Failure(new Error("organizations.members.change_role_invalid", ex.Message));
        }
    }
}
