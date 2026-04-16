namespace Voluntrio.Common.Infrastructure.Integration;

/// <summary>
/// Configuration options for NATS integration publishing.
/// </summary>
public sealed class NatsIntegrationOptions
{
    public const string SectionName = "Nats";

    /// <summary>
    /// NATS server URL.
    /// </summary>
    public string Url { get; set; } = "nats://localhost:4222";

    /// <summary>
    /// Prefix used for integration event subjects.
    /// </summary>
    public string SubjectPrefix { get; set; } = "voluntrio.integration";
}
