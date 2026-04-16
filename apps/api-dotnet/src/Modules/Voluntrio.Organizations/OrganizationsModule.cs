using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Behaviors;
using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Common.Persistence.Stores;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;
using Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;
using Voluntrio.Organizations.Application.UseCases.CreateOrganization;
using Voluntrio.Organizations.Application.UseCases.GetOrganization;
using Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;
using Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.Organizations;

public static class OrganizationsModule
{
    public static IServiceCollection AddOrganizationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrganizationsDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=voluntrio;Username=postgres;Password=postgres";

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(OrganizationsModule).Assembly.GetName().Name);
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "org");
            }));

        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IModelBuilderConfiguration, OrganizationsModelBuilderConfiguration>();

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddScoped<ICommandHandler<CreateOrganizationCommand, Result<CreateOrganizationResponse>>, CreateOrganizationHandler>();
        services.AddScoped<ICommandHandler<AddOrganizationMemberCommand, Result<AddOrganizationMemberResponse>>, AddOrganizationMemberHandler>();
        services.AddScoped<ICommandHandler<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>, ChangeMemberRoleHandler>();
        services.AddScoped<ICommandHandler<RemoveOrganizationMemberCommand, Result<RemoveOrganizationMemberResponse>>, RemoveOrganizationMemberHandler>();
        services.AddScoped<IQueryHandler<GetOrganizationQuery, Result<GetOrganizationResponse>>, GetOrganizationHandler>();
        services.AddScoped<IQueryHandler<ListOrganizationMembersQuery, Result<ListOrganizationMembersResponse>>, ListOrganizationMembersHandler>();

        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingCommandBehavior<,>));
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(TransactionCommandBehavior<,>));
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(IdempotencyCommandBehavior<,>));
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(LoggingQueryBehavior<,>));

        services.AddScoped<IOutboxStore, EfOutboxStore>();
        services.AddScoped<IInboxStore, EfInboxStore>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

        RegisterEndpoints(services);

        return services;
    }

    public static IEndpointRouteBuilder MapOrganizationsModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations").WithTags("Organizations");

        var endpoints = app.ServiceProvider.GetRequiredService<IEnumerable<IOrganizationsEndpoint>>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(group);
        }

        return app;
    }

    private static void RegisterEndpoints(IServiceCollection services)
    {
        var endpointType = typeof(IOrganizationsEndpoint);
        var assembly = typeof(OrganizationsModule).Assembly;

        var endpointImplementations = assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                endpointType.IsAssignableFrom(type));

        foreach (var endpointImplementation in endpointImplementations)
        {
            services.AddSingleton(endpointType, endpointImplementation);
        }
    }
}
