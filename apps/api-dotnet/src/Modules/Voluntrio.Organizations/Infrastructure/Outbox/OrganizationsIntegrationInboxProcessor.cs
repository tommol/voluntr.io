using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voluntrio.Common.Infrastructure.Inbox;

namespace Voluntrio.Organizations.Infrastructure.Outbox;

public sealed class OrganizationsIntegrationInboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OrganizationsIntegrationInboxProcessor> logger)
    : ModuleIntegrationInboxProcessor(scopeFactory, logger)
{
    protected override string IntegrationEventTypePrefix => "Voluntrio.Organizations.Contracts.IntegrationEvents.";
}
