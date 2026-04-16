namespace Voluntrio.Organizations.Application.Federation;

/// <summary>
/// Provides federation-related lookups based on organization hierarchy.
/// Federation is defined by the root organization in the parent chain.
/// </summary>
public interface IFederationMembershipService
{
    Task<Guid> GetRootOrganizationId(Guid organizationId, CancellationToken cancellationToken);

    Task<Guid?> GetUserActiveFederationId(Guid userId, CancellationToken cancellationToken);
}
