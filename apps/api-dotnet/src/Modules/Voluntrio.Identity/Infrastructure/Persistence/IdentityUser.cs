namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class IdentityUser
{
    public Guid Id { get; private set; }

    public string? KindeUserId { get; private set; }

    public string? Email { get; private set; }

    public string? GivenName { get; private set; }

    public string? FamilyName { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private IdentityUser()
    {
    }

    public static IdentityUser Create(
        Guid id,
        string? kindeUserId,
        string? email,
        string? givenName,
        string? familyName,
        DateTime utcNow)
    {
        return new IdentityUser
        {
            Id = id,
            KindeUserId = kindeUserId,
            Email = email,
            GivenName = givenName,
            FamilyName = familyName,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public void UpdateProfile(string? email, string? givenName, string? familyName, DateTime utcNow)
    {
        Email = email;
        GivenName = givenName;
        FamilyName = familyName;
        UpdatedAtUtc = utcNow;
    }

    public void LinkKindeUser(string kindeUserId, DateTime utcNow)
    {
        KindeUserId = kindeUserId;
        UpdatedAtUtc = utcNow;
    }
}
