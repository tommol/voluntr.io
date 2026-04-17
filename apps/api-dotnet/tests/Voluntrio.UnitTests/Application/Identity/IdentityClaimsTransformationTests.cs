using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Application.Security;
using Voluntrio.Identity.Infrastructure.Persistence;
using Voluntrio.Identity.Infrastructure.Security;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.UnitTests.Application.Identity;

public sealed class IdentityClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsync_ShouldProvisionInternalUserAndExposeCurrentUser()
    {
        await using var dbContext = CreateDbContext();
        var service = new IdentityMappingService(dbContext);
        var transformation = new IdentityClaimsTransformation(service);

        var principal = CreatePrincipal("kinde-user-123", "first@voluntr.io", "First", "User");
        var transformed = await transformation.TransformAsync(principal);

        var claimValue = transformed.FindFirstValue(IdentityClaimTypes.InternalUserId);
        Assert.True(Guid.TryParse(claimValue, out var internalUserId));

        var storedMapping = await dbContext.Set<ExternalIdentity>()
            .SingleAsync(x => x.Provider == "kinde" && x.ExternalSubject == "kinde-user-123");
        var storedUser = await dbContext.Set<IdentityUser>()
            .SingleAsync(x => x.Id == internalUserId);

        Assert.Equal(internalUserId, storedMapping.InternalUserId);
        Assert.Equal("first@voluntr.io", storedMapping.Email);
        Assert.Equal("first@voluntr.io", storedUser.Email);
        Assert.Equal("First", storedUser.GivenName);
        Assert.Equal("User", storedUser.FamilyName);
        Assert.Equal("kinde-user-123", storedUser.KindeUserId);

        var httpContext = new DefaultHttpContext { User = transformed };
        var currentUser = new HttpContextCurrentUser(new HttpContextAccessor { HttpContext = httpContext });

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal("kinde-user-123", currentUser.ExternalSubject);
        Assert.Equal(internalUserId, currentUser.InternalUserId);
    }

    [Fact]
    public async Task TransformAsync_ShouldReuseExistingMappingForSameSubject()
    {
        await using var dbContext = CreateDbContext();
        var service = new IdentityMappingService(dbContext);
        var transformation = new IdentityClaimsTransformation(service);

        var first = await transformation.TransformAsync(CreatePrincipal("kinde-user-555", "first@voluntr.io", "First", "User"));
        var second = await transformation.TransformAsync(CreatePrincipal("kinde-user-555", "updated@voluntr.io", "Updated", "User"));

        var firstInternalId = first.FindFirstValue(IdentityClaimTypes.InternalUserId);
        var secondInternalId = second.FindFirstValue(IdentityClaimTypes.InternalUserId);

        Assert.Equal(firstInternalId, secondInternalId);

        var storedMapping = await dbContext.Set<ExternalIdentity>()
            .SingleAsync(x => x.Provider == "kinde" && x.ExternalSubject == "kinde-user-555");
        var storedUser = await dbContext.Set<IdentityUser>()
            .SingleAsync(x => x.Id == storedMapping.InternalUserId);

        Assert.Equal("updated@voluntr.io", storedMapping.Email);
        Assert.Equal("updated@voluntr.io", storedUser.Email);
        Assert.Equal("Updated", storedUser.GivenName);
        Assert.Equal("User", storedUser.FamilyName);
        Assert.Equal("kinde-user-555", storedUser.KindeUserId);
    }

    private static ClaimsPrincipal CreatePrincipal(string sub, string email, string givenName, string familyName)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", sub),
            new Claim(ClaimTypes.Email, email),
            new Claim("given_name", givenName),
            new Claim("family_name", familyName),
        ],
        authenticationType: "Bearer");

        return new ClaimsPrincipal(identity);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration(),
            new IdentityModelBuilderConfiguration()
        };

        return new AppDbContext(options, modelConfigurations);
    }
}
