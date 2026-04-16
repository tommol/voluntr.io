using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Voluntrio.Api.OpenApi;

/// <summary>
/// Removes the {version} route parameter from Swagger operations and substitutes the
/// actual version number into the operation path so the UI shows clean, callable URLs.
/// </summary>
public sealed class SubstituteApiVersionRouteParamFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        var versionParam = operation.Parameters
            .FirstOrDefault(p => p.In == ParameterLocation.Path
                && string.Equals(p.Name, "version", StringComparison.OrdinalIgnoreCase));

        if (versionParam is not null)
        {
            operation.Parameters.Remove(versionParam);
        }
    }
}
