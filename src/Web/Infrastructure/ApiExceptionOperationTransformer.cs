using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ERP_Government.Web.Infrastructure;

/// <summary>
/// Adds standard error responses to every OpenAPI operation. A 400 Bad Request is added to all
/// operations because every request passes through <c>ValidationBehaviour</c> in the MediatR
/// pipeline. 401 Unauthorized and 403 Forbidden are added only to operations that carry
/// <see cref="IAuthorizeData"/> metadata. 429 is added as a reserved classification.
/// Each error response includes an <c>application/problem+json</c> content block with the
/// RFC 9457 ProblemDetails schema.
/// </summary>
internal sealed class ApiExceptionOperationTransformer : IOpenApiOperationTransformer
{
    private static OpenApiSchema CreateProblemDetailsSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Problem type URI" },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Short human-readable summary" },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = "HTTP status code" },
            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Safe explanation" },
            ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Safe request path or occurrence identifier" },
            ["code"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Stable machine-readable error code" },
            ["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Correlation identifier for diagnostics" },
            ["errors"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "Optional map of canonical field paths to arrays of safe messages",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String }
                }
            }
        }
    };

    private static OpenApiResponse CreateProblemDetailsResponse(string description)
    {
        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = CreateProblemDetailsSchema()
                }
            }
        };
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses ??= [];

        operation.Responses.TryAdd("400", CreateProblemDetailsResponse(
            "Bad Request — validation or business rule failure"));

        var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAuthorizeData);

        if (requiresAuth)
        {
            operation.Responses.TryAdd("401", CreateProblemDetailsResponse(
                "Unauthorized — authentication required"));
            operation.Responses.TryAdd("403", CreateProblemDetailsResponse(
                "Forbidden — insufficient permissions"));
        }

        operation.Responses.TryAdd("429", CreateProblemDetailsResponse(
            "Rate Limit Exceeded — reserved classification (not yet implemented)"));

        return Task.CompletedTask;
    }
}
