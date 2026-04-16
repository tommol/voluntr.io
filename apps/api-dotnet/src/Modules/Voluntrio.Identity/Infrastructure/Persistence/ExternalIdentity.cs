namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class ExternalIdentity
{
    public string Provider { get; private set; } = null!;

    public string ExternalSubject { get; private set; } = null!;

    public Guid InternalUserId { get; private set; }

    public string? Email { get; private set; }

    public string? GivenName { get; private set; }

    public string? FamilyName { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private ExternalIdentity()
    {
    }

    public static ExternalIdentity Create(
        string provider,
        string externalSubject,
        Guid internalUserId,
        string? email,
        string? givenName,
        string? familyName,
        DateTime updatedAtUtc)
    {
        return new ExternalIdentity
        {
            Provider = provider,
            ExternalSubject = externalSubject,
            InternalUserId = internalUserId,
            Email = email,
            GivenName = givenName,
            FamilyName = familyName,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    public void UpdateProfile(string? email, string? givenName, string? familyName, DateTime updatedAtUtc)
    {
        Email = email;
        GivenName = givenName;
        FamilyName = familyName;
        UpdatedAtUtc = updatedAtUtc;
    }
}
