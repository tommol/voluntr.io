using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Application.Federation;
using Voluntrio.Organizations.Contracts.UseCases.InviteOrganizationMember;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Application.UseCases.InviteOrganizationMember;

public sealed class InviteOrganizationMemberHandler(
    AppDbContext dbContext,
    IIdentityUserProvisioningService identityUserProvisioningService,
    IFederationMembershipService federationMembershipService,
    IClock clock) : ICommandHandler<InviteOrganizationMemberCommand, Result<InviteOrganizationMemberResponse>>
{
    public async Task<Result<InviteOrganizationMemberResponse>> Handle(
        InviteOrganizationMemberCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<InviteOrganizationMemberResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        if (!organization.CanManageMembers(command.ActorUserId))
        {
            return Result<InviteOrganizationMemberResponse>.Failure(
                new Error("organizations.members.add_invalid", "Only owner or admin can manage organization members."));
        }

        try
        {
            var internalUserId = await identityUserProvisioningService.EnsureInternalUserForEmailAsync(
                command.Email,
                command.GivenName,
                command.FamilyName,
                cancellationToken);

            var targetFederationId = await federationMembershipService.GetRootOrganizationId(command.OrganizationId, cancellationToken);
            var userFederationId = await federationMembershipService.GetUserActiveFederationId(internalUserId, cancellationToken);

            if (userFederationId.HasValue && userFederationId.Value != targetFederationId)
            {
                return Result<InviteOrganizationMemberResponse>.Failure(
                    new Error(
                        "organizations.members.cross_federation_forbidden",
                        "User can only be an active member within a single federation."));
            }

            organization.AddMember(command.ActorUserId, internalUserId, new MembershipRole(command.Role), clock.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            var membership = organization.Members.Single(x => x.UserId == internalUserId);

            return Result<InviteOrganizationMemberResponse>.Success(
                new InviteOrganizationMemberResponse(organization.Id, membership.ToDto()));
        }
        catch (DomainException ex)
        {
            return Result<InviteOrganizationMemberResponse>.Failure(
                new Error("organizations.members.add_invalid", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result<InviteOrganizationMemberResponse>.Failure(
                new Error("identity.provisioning.invalid_configuration", ex.Message));
        }
        catch (HttpRequestException ex)
        {
            return Result<InviteOrganizationMemberResponse>.Failure(
                new Error("identity.provisioning.external_error", ex.Message));
        }
    }
}
