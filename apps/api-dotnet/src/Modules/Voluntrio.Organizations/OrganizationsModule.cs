using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Behaviors;
using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.DomainEvents;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Common.Application.Integration;
using Voluntrio.Common.Application.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Common.Persistence.Integration;
using Voluntrio.Common.Persistence.Stores;
using Voluntrio.Common.Infrastructure.Integration;
using Voluntrio.Common.Infrastructure.Outbox;
using Voluntrio.Organizations.Application.Federation;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;
using Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;
using Voluntrio.Organizations.Application.UseCases.CreateOrganization;
using Voluntrio.Organizations.Application.UseCases.CreateSubOrganization;
using Voluntrio.Organizations.Application.UseCases.GetMyOrganizations;
using Voluntrio.Organizations.Application.UseCases.GetOrganization;
using Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;
using Voluntrio.Organizations.Application.UseCases.ListSubOrganizations;
using Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;
using Voluntrio.Organizations.Contracts.UseCases.CreateSubOrganization;
using Voluntrio.Organizations.Contracts.UseCases.GetMyOrganizations;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;
using Voluntrio.Organizations.Contracts.UseCases.ListSubOrganizations;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;
using Voluntrio.Organizations.Infrastructure.Integration;
using Voluntrio.Organizations.Infrastructure.Outbox;
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
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "common");
            }));

        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddIntegrationAbstractions();
        services
            .AddOptions<OutboxProcessingOptions>()
            .Bind(configuration.GetSection(OutboxProcessingOptions.SectionName));

        services.AddSingleton<IModelBuilderConfiguration, OrganizationsModelBuilderConfiguration>();
        services.AddSingleton<IIntegrationEventSerializer, SystemTextJsonIntegrationEventSerializer>();
        services.AddSingleton<IDomainEventToIntegrationEventMapper, OrganizationsDomainEventToIntegrationEventMapper>();
        services.AddNatsIntegrationPublishing(configuration);
        services.AddHostedService<NatsIntegrationInboxSubscriber>();
        services.AddHostedService<OrganizationsDomainOutboxProcessor>();
        services.AddHostedService<IntegrationOutboxProcessor>();
        services.AddHostedService<OrganizationsIntegrationInboxProcessor>();

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IFederationMembershipService, FederationMembershipService>();
        services.AddScoped<IOrganizationMembershipAuthorization, OrganizationMembershipAuthorization>();

        services.AddScoped<ICommandHandler<CreateOrganizationCommand, Result<CreateOrganizationResponse>>, CreateOrganizationHandler>();
        services.AddScoped<ICommandHandler<CreateSubOrganizationCommand, Result<CreateSubOrganizationResponse>>, CreateSubOrganizationHandler>();
        services.AddScoped<ICommandHandler<AddOrganizationMemberCommand, Result<AddOrganizationMemberResponse>>, AddOrganizationMemberHandler>();
        services.AddScoped<ICommandHandler<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>, ChangeMemberRoleHandler>();
        services.AddScoped<ICommandHandler<RemoveOrganizationMemberCommand, Result<RemoveOrganizationMemberResponse>>, RemoveOrganizationMemberHandler>();
        services.AddScoped<IQueryHandler<GetOrganizationQuery, Result<GetOrganizationResponse>>, GetOrganizationHandler>();
        services.AddScoped<IQueryHandler<GetMyOrganizationsQuery, Result<GetMyOrganizationsResponse>>, GetMyOrganizationsHandler>();
        services.AddScoped<IQueryHandler<ListOrganizationMembersQuery, Result<ListOrganizationMembersResponse>>, ListOrganizationMembersHandler>();
        services.AddScoped<IQueryHandler<ListSubOrganizationsQuery, Result<ListSubOrganizationsResponse>>, ListSubOrganizationsHandler>();

        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingCommandBehavior<,>));
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(TransactionCommandBehavior<,>));
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(IdempotencyCommandBehavior<,>));
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(LoggingQueryBehavior<,>));

        services.AddScoped<IOutboxStore, EfOutboxStore>();
        services.AddScoped<IInboxStore, EfInboxStore>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

        services.AddEndpointsFromAssembly(typeof(OrganizationsModule).Assembly);

        return services;
    }

    public static IEndpointRouteBuilder MapOrganizationsModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/organizations")
            .WithApiVersionSet(versionSet)
            .WithTags("Organizations")
            .RequireAuthorization();

        group.MapRegisteredEndpoints();
        return app;
    }
}
