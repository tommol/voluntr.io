using System.Net.Http.Headers;
using System.Net.Http.Json;
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
            localUser.UpdateProfile(normalizedEmail, givenName, familyName, DateTime.UtcNow);

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
                        DateTime.UtcNow));
            }
            else
            {
                existingMappedUser.UpdateProfile(
                    normalizedEmail,
                    givenName ?? kindeUser.FirstName,
                    familyName ?? kindeUser.LastName,
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
            : new KindeUser(matching.Id, matching.Email, matching.FirstName, matching.LastName);
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

        return new KindeUser(payload.Id, email, givenName, familyName);
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

    private sealed record KindeUser(string Id, string? Email, string? FirstName, string? LastName);

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

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }
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
}
