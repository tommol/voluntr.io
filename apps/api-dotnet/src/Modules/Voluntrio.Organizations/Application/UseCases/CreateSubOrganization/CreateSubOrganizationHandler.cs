using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Contracts.UseCases.CreateSubOrganization;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Application.UseCases.CreateSubOrganization;

/// <summary>
/// Handles creating a new sub-organization under a parent organization.
/// </summary>
public sealed class CreateSubOrganizationHandler(
    AppDbContext dbContext,
    IClock clock) : ICommandHandler<CreateSubOrganizationCommand, Result<CreateSubOrganizationResponse>>
{
    public async Task<Result<CreateSubOrganizationResponse>> Handle(
        CreateSubOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var parentOrganization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == command.ParentOrganizationId, cancellationToken);

        if (parentOrganization is null)
        {
            return Result<CreateSubOrganizationResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        if (!parentOrganization.CanManageMembers(command.ActorUserId))
        {
            return Result<CreateSubOrganizationResponse>.Failure(
                new Error(
                    "organizations.sub_organizations.create_forbidden",
                    "Only owner or admin can create sub-organizations."));
        }

        try
        {
            var name = OrganizationName.Create(command.Name);
            var slug = await CreateUniqueSlugAsync(name, cancellationToken);

            var organization = Organization.Create(
                name,
                slug,
                command.ActorUserId,
                clock.UtcNow,
                command.ParentOrganizationId);

            dbContext.Set<Organization>().Add(organization);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<CreateSubOrganizationResponse>.Success(
                new CreateSubOrganizationResponse(
                    organization.Id,
                    command.ParentOrganizationId,
                    organization.Name.Value,
                    organization.Slug.Value,
                    organization.Status.Value,
                    organization.OwnerUserId,
                    organization.CreatedAtUtc));
        }
        catch (DomainException ex)
        {
            return Result<CreateSubOrganizationResponse>.Failure(
                new Error("organizations.sub_organizations.create_invalid", ex.Message));
        }
    }

    private async Task<OrganizationSlug> CreateUniqueSlugAsync(OrganizationName name, CancellationToken cancellationToken)
    {
        var baseSlug = OrganizationSlug.FromName(name).Value;
        var candidate = baseSlug;
        var counter = 1;

        while (await dbContext.Set<Organization>().AnyAsync(x => x.Slug == OrganizationSlug.Create(candidate), cancellationToken))
        {
            counter++;
            candidate = $"{baseSlug}-{counter}";
        }

        return OrganizationSlug.Create(candidate);
    }
}
