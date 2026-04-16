using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;

/// <summary>
/// Handles adding a new member to an organization.
/// </summary>
public sealed class AddOrganizationMemberHandler(
    AppDbContext dbContext,
    IClock clock) : ICommandHandler<AddOrganizationMemberCommand, Result<AddOrganizationMemberResponse>>
{
    /// <summary>
    /// Handles the add organization member command.
    /// </summary>
    /// <param name="command">The add organization member command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The member addition result containing the member data.</returns>
    public async Task<Result<AddOrganizationMemberResponse>> Handle(AddOrganizationMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<AddOrganizationMemberResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        try
        {
            organization.AddMember(command.ActorUserId, command.UserId, new MembershipRole(command.Role), clock.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            var membership = organization.Members.Single(x => x.UserId == command.UserId);

            return Result<AddOrganizationMemberResponse>.Success(
                new AddOrganizationMemberResponse(organization.Id, membership.ToDto()));
        }
        catch (DomainException ex)
        {
            return Result<AddOrganizationMemberResponse>.Failure(new Error("organizations.members.add_invalid", ex.Message));
        }
    }
}
