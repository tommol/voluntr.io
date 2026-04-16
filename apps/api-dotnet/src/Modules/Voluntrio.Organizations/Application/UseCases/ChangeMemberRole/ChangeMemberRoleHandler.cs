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

/// <summary>
/// Handles changing a member's role within an organization.
/// </summary>
public sealed class ChangeMemberRoleHandler(
    AppDbContext dbContext,
    IClock clock) : ICommandHandler<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>
{
    /// <summary>
    /// Handles the change member role command.
    /// </summary>
    /// <param name="command">The change member role command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role change result containing the updated member data.</returns>
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
