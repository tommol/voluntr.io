using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Organizations.Application.UseCases.CreateOrganization;

/// <summary>
/// Handles the creation of a new organization.
/// </summary>
public sealed class CreateOrganizationHandler(
    AppDbContext dbContext,
    IClock clock) : ICommandHandler<CreateOrganizationCommand, Result<CreateOrganizationResponse>>
{
    /// <summary>
    /// Handles the create organization command.
    /// </summary>
    /// <param name="command">The create organization command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The organization creation result containing the new organization data.</returns>
    public async Task<Result<CreateOrganizationResponse>> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var name = OrganizationName.Create(command.Name);
            var slug = await CreateUniqueSlugAsync(name, cancellationToken);

            var organization = Organization.Create(name, slug, command.OwnerUserId, clock.UtcNow);

            dbContext.Set<Organization>().Add(organization);
            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new CreateOrganizationResponse(
                organization.Id,
                organization.Name.Value,
                organization.Slug.Value,
                organization.Status.Value,
                organization.OwnerUserId,
                organization.CreatedAtUtc);

            return Result<CreateOrganizationResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<CreateOrganizationResponse>.Failure(new Error("organizations.create.invalid", ex.Message));
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
