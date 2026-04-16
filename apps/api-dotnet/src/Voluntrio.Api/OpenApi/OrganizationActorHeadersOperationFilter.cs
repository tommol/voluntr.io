using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Voluntrio.Api.OpenApi;

/// <summary>
/// Adds request headers used by organization write endpoints in Swagger UI.
/// </summary>
public sealed class OrganizationActorHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath;
        var httpMethod = context.ApiDescription.HttpMethod;

        if (string.IsNullOrWhiteSpace(relativePath) || string.IsNullOrWhiteSpace(httpMethod))
        {
            return;
        }

        var isOrganizationsEndpoint = relativePath.StartsWith("api/v", StringComparison.OrdinalIgnoreCase)
            && relativePath.Contains("/organizations", StringComparison.OrdinalIgnoreCase);
        var isWriteMethod = httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)
            || httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase)
            || httpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase)
            || httpMethod.Equals("PATCH", StringComparison.OrdinalIgnoreCase);

        if (!isOrganizationsEndpoint || !isWriteMethod)
        {
            return;
        }

        operation.Parameters ??= new List<IOpenApiParameter>();

        AddHeaderIfMissing(
            operation,
            "X-Idempotency-Key",
            required: false,
            "Optional idempotency key for safe retries.",
            "test-idempotency-key-001");
    }

    private static void AddHeaderIfMissing(OpenApiOperation operation, string name, bool required, string description, string sampleValue)
    {
        var parameters = operation.Parameters ??= new List<IOpenApiParameter>();

        var alreadyExists = parameters.Any(parameter =>
            parameter.In == ParameterLocation.Header
            && string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return;
        }

        parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Header,
            Required = required,
            Description = description,
            Example = JsonValue.Create(sampleValue),
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Default = JsonValue.Create(sampleValue)
            }
        });
    }
}
