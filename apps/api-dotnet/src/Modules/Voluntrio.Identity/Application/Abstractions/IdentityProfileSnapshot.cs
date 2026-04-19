namespace Voluntrio.Identity.Application.Abstractions;

public sealed record IdentityProfileSnapshot(
    string? Email,
    string? GivenName,
    string? FamilyName,
    string? PictureUrl);
