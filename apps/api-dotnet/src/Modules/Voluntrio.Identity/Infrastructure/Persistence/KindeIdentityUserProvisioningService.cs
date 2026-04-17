using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class KindeIdentityUserProvisioningService(
    AppDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IIdentityUserProvisioningService
{
    private const string ProviderName = "kinde";

    public async Task<IdentityProfileSnapshot?> GetProfileByExternalSubjectAsync(
        string provider,
        string externalSubject,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(provider, ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var kindeUser = await GetKindeUserByIdAsync(externalSubject, cancellationToken)
            ?? await GetKindeUserByUsersFilterAsync(externalSubject, cancellationToken);
        if (kindeUser is null)
        {
            return null;
        }

        return new IdentityProfileSnapshot(
            kindeUser.Email,
            kindeUser.FirstName,
            kindeUser.LastName,
            kindeUser.Picture);
    }

    public async Task<Guid> EnsureInternalUserForEmailAsync(
        string email,
        string? givenName,
        string? familyName,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new InvalidOperationException("Email is required to provision user identity.");
        }

        var localUser = await dbContext.Set<IdentityUser>()
            .SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (localUser is not null)
        {
            localUser.UpdateProfile(
                normalizedEmail,
                givenName,
                familyName,
                IdentityAvatarUrlResolver.Resolve(normalizedEmail, localUser.AvatarUrl),
                DateTime.UtcNow);

            var localMapping = await dbContext.Set<ExternalIdentity>()
                .SingleOrDefaultAsync(
                    x => x.Provider == ProviderName && x.InternalUserId == localUser.Id,
                    cancellationToken);

            if (localMapping is not null)
            {
                if (!string.IsNullOrWhiteSpace(localMapping.ExternalSubject)
                    && !string.Equals(localUser.KindeUserId, localMapping.ExternalSubject, StringComparison.Ordinal))
                {
                    localUser.LinkKindeUser(localMapping.ExternalSubject, DateTime.UtcNow);
                }

                localMapping.UpdateProfile(
                    normalizedEmail,
                    givenName ?? localMapping.GivenName,
                    familyName ?? localMapping.FamilyName,
                    DateTime.UtcNow);
                return localUser.Id;
            }

            var kindeUserForLocal = await FindKindeUserByEmailAsync(normalizedEmail, cancellationToken)
                ?? await CreateKindeUserAsync(normalizedEmail, givenName, familyName, cancellationToken);

            var mappedIdentity = await dbContext.Set<ExternalIdentity>()
                .SingleOrDefaultAsync(
                    x => x.Provider == ProviderName && x.ExternalSubject == kindeUserForLocal.Id,
                    cancellationToken);

            if (mappedIdentity is not null)
            {
                if (!string.Equals(localUser.KindeUserId, mappedIdentity.ExternalSubject, StringComparison.Ordinal))
                {
                    localUser.LinkKindeUser(mappedIdentity.ExternalSubject, DateTime.UtcNow);
                }

                return mappedIdentity.InternalUserId;
            }

            localUser.LinkKindeUser(kindeUserForLocal.Id, DateTime.UtcNow);
            dbContext.Set<ExternalIdentity>().Add(
                ExternalIdentity.Create(
                    ProviderName,
                    kindeUserForLocal.Id,
                    localUser.Id,
                    normalizedEmail,
                    givenName ?? kindeUserForLocal.FirstName,
                    familyName ?? kindeUserForLocal.LastName,
                    DateTime.UtcNow));

            return localUser.Id;
        }

        var kindeUser = await FindKindeUserByEmailAsync(normalizedEmail, cancellationToken)
            ?? await CreateKindeUserAsync(normalizedEmail, givenName, familyName, cancellationToken);

        var existingMapping = await dbContext.Set<ExternalIdentity>()
            .SingleOrDefaultAsync(
                x => x.Provider == ProviderName && x.ExternalSubject == kindeUser.Id,
                cancellationToken);

        if (existingMapping is not null)
        {
            var existingMappedUser = await dbContext.Set<IdentityUser>()
                .SingleOrDefaultAsync(x => x.Id == existingMapping.InternalUserId, cancellationToken);

            if (existingMappedUser is null)
            {
                dbContext.Set<IdentityUser>().Add(
                    IdentityUser.Create(
                        existingMapping.InternalUserId,
                        kindeUser.Id,
                        normalizedEmail,
                        givenName ?? kindeUser.FirstName,
                        familyName ?? kindeUser.LastName,
                        IdentityAvatarUrlResolver.Resolve(normalizedEmail, kindeUser.Picture),
                        DateTime.UtcNow));
            }
            else
            {
                existingMappedUser.UpdateProfile(
                    normalizedEmail,
                    givenName ?? kindeUser.FirstName,
                    familyName ?? kindeUser.LastName,
                    IdentityAvatarUrlResolver.Resolve(normalizedEmail, kindeUser.Picture),
                    DateTime.UtcNow);

                if (!string.Equals(existingMappedUser.KindeUserId, kindeUser.Id, StringComparison.Ordinal))
                {
                    existingMappedUser.LinkKindeUser(kindeUser.Id, DateTime.UtcNow);
                }
            }

            existingMapping.UpdateProfile(
                normalizedEmail,
                givenName ?? kindeUser.FirstName,
                familyName ?? kindeUser.LastName,
                DateTime.UtcNow);

            return existingMapping.InternalUserId;
        }

        var internalUserId = Guid.CreateVersion7();
        dbContext.Set<IdentityUser>().Add(
            IdentityUser.Create(
                internalUserId,
                kindeUser.Id,
                normalizedEmail,
                givenName ?? kindeUser.FirstName,
                familyName ?? kindeUser.LastName,
                IdentityAvatarUrlResolver.Resolve(normalizedEmail, kindeUser.Picture),
                DateTime.UtcNow));

        dbContext.Set<ExternalIdentity>().Add(
            ExternalIdentity.Create(
                ProviderName,
                kindeUser.Id,
                internalUserId,
                normalizedEmail,
                givenName ?? kindeUser.FirstName,
                familyName ?? kindeUser.LastName,
                DateTime.UtcNow));

        return internalUserId;
    }

    private async Task<KindeUser?> FindKindeUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);
        var issuer = GetIssuer();
        using var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{issuer}/api/v1/users?email={Uri.EscapeDataString(email)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<KindeUsersResponse>(cancellationToken);
        if (payload?.Users is null || payload.Users.Count == 0)
        {
            return null;
        }

        var matching = payload.Users.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.Email)
            && string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));

        return matching is null
            ? null
            : new KindeUser(matching.Id, matching.Email, matching.FirstName, matching.LastName, matching.Picture);
    }

    private async Task<KindeUser?> GetKindeUserByIdAsync(string kindeUserId, CancellationToken cancellationToken)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);
        var issuer = GetIssuer();
        using var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{issuer}/api/v1/user?id={Uri.EscapeDataString(kindeUserId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        if (payload is null)
        {
            return null;
        }

        var root = payload.RootElement;
        if (root.TryGetProperty("user", out var nestedUser) && nestedUser.ValueKind == JsonValueKind.Object)
        {
            return ParseKindeUserFromElement(nestedUser);
        }

        return root.ValueKind == JsonValueKind.Object
            ? ParseKindeUserFromElement(root)
            : null;
    }

    private async Task<KindeUser?> GetKindeUserByUsersFilterAsync(string kindeUserId, CancellationToken cancellationToken)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);
        var issuer = GetIssuer();
        using var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{issuer}/api/v1/users?user_id={Uri.EscapeDataString(kindeUserId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        if (payload is null)
        {
            return null;
        }

        var users = ExtractUsersArray(payload.RootElement);
        foreach (var userElement in users)
        {
            var user = ParseKindeUserFromElement(userElement);
            if (user is not null && string.Equals(user.Id, kindeUserId, StringComparison.Ordinal))
            {
                return user;
            }
        }

        return null;
    }

    private async Task<KindeUser> CreateKindeUserAsync(
        string email,
        string? givenName,
        string? familyName,
        CancellationToken cancellationToken)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);
        var issuer = GetIssuer();

        using var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{issuer}/api/v1/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(
            new
            {
                profile = new
                {
                    given_name = givenName,
                    family_name = familyName
                },
                identities = new object[]
                {
                    new
                    {
                        type = "email",
                        is_verified = false,
                        details = new
                        {
                            email
                        }
                    }
                }
            });

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<KindeCreateUserResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload?.Id))
        {
            throw new InvalidOperationException("Kinde create user response did not contain user id.");
        }

        return new KindeUser(payload.Id, email, givenName, familyName, null);
    }

    private async Task<string> GetManagementApiTokenAsync(CancellationToken cancellationToken)
    {
        var issuer = GetIssuer();
        var clientId = configuration["Authentication:Kinde:ManagementApi:ClientId"];
        var clientSecret = configuration["Authentication:Kinde:ManagementApi:ClientSecret"];
        var audience = configuration["Authentication:Kinde:ManagementApi:Audience"] ?? $"{issuer}/api";

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("Kinde Management API credentials are not configured.");
        }

        using var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{issuer}/oauth2/token")
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["audience"] = audience
                })
        };

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenPayload = await response.Content.ReadFromJsonAsync<KindeTokenResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(tokenPayload?.AccessToken))
        {
            throw new InvalidOperationException("Unable to obtain Kinde Management API access token.");
        }

        return tokenPayload.AccessToken;
    }

    private string GetIssuer()
    {
        var issuer = configuration["Authentication:Kinde:Issuer"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("Authentication:Kinde:Issuer is not configured.");
        }

        return issuer;
    }

    private sealed record KindeUser(string Id, string? Email, string? FirstName, string? LastName, string? Picture);

    private sealed class KindeUsersResponse
    {
        [JsonPropertyName("users")]
        public List<KindeUserData>? Users { get; init; }
    }

    private sealed class KindeUserData
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = null!;

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("preferred_email")]
        public string? PreferredEmail
        {
            init => Email = value;
        }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; init; }

        [JsonPropertyName("picture")]
        public string? Picture { get; init; }

        [JsonPropertyName("identities")]
        public List<KindeIdentityData>? Identities { get; init; }
    }

    private sealed class KindeCreateUserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    private sealed class KindeTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }
    }

    private sealed class KindeUserByIdResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("preferred_email")]
        public string? PreferredEmail { get; init; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; init; }

        [JsonPropertyName("picture")]
        public string? Picture { get; init; }

        [JsonPropertyName("identities")]
        public List<KindeIdentityData>? Identities { get; init; }
    }

    private sealed class KindeIdentityData
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("identity")]
        public string? Identity { get; init; }
    }

    private static string? ExtractEmailFromIdentities(List<KindeIdentityData>? identities)
    {
        var emailIdentity = identities?.FirstOrDefault(x =>
            string.Equals(x.Type, "email", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(x.Identity));
        return emailIdentity?.Identity;
    }

    private static string? CoalesceNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static KindeUser? ParseKindeUserFromElement(JsonElement user)
    {
        var id = CoalesceNonEmpty(
            ReadString(user, "id"),
            ReadString(user, "user_id"));
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var email = CoalesceNonEmpty(
            ReadString(user, "preferred_email"),
            ReadString(user, "email"),
            ExtractEmailFromIdentityElements(user));
        var givenName = CoalesceNonEmpty(
            ReadString(user, "first_name"),
            ReadString(user, "given_name"));
        var familyName = CoalesceNonEmpty(
            ReadString(user, "last_name"),
            ReadString(user, "family_name"));
        var picture = CoalesceNonEmpty(
            ReadString(user, "picture"),
            ReadString(user, "picture_url"),
            ReadString(user, "avatar_url"));

        return new KindeUser(id, email, givenName, familyName, picture);
    }

    private static IEnumerable<JsonElement> ExtractUsersArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    yield return item;
                }
            }

            yield break;
        }

        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("users", out var users)
            && users.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in users.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    yield return item;
                }
            }
        }
    }

    private static string? ExtractEmailFromIdentityElements(JsonElement user)
    {
        if (!user.TryGetProperty("identities", out var identities) || identities.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var identity in identities.EnumerateArray())
        {
            if (identity.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var type = ReadString(identity, "type");
            if (!string.Equals(type, "email", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var email = CoalesceNonEmpty(
                ReadString(identity, "identity"),
                ReadString(identity, "email"));
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            if (identity.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Object)
            {
                var detailsEmail = ReadString(details, "email");
                if (!string.IsNullOrWhiteSpace(detailsEmail))
                {
                    return detailsEmail;
                }
            }
        }

        return null;
    }

    private static string? ReadString(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}
