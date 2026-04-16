using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Identity.Application.Abstractions;
using Voluntrio.Identity.Application.Security;
using Voluntrio.Identity.Infrastructure.Persistence;
using Voluntrio.Identity.Infrastructure.Security;

namespace Voluntrio.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<IModelBuilderConfiguration, IdentityModelBuilderConfiguration>();
        services.AddScoped<IIdentityMappingService, IdentityMappingService>();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<IClaimsTransformation, IdentityClaimsTransformation>();

        return services;
    }
}
