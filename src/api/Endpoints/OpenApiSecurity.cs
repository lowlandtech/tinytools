using LowlandTech.TinyTools.Api.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>
/// Makes the published contract say what the host requires. The route group demands the API-key
/// policy, but without this the document declared a template API anybody could call: no security
/// scheme, so a generated client would never send the header. The scheme and the requirement are
/// declared once, here, and the drift check compares the served document against a committed copy.
/// </summary>
public static class OpenApiSecurity
{
    public const string SchemeName = "TinytoolsApiKey";

    public static OpenApiOptions DescribeApiKey(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = ApiKeyPolicy.HeaderName,
                Description = "The deployment's API key. Every template operation requires it outside Development.",
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, document)] = [],
            });

            return Task.CompletedTask;
        });

        return options;
    }
}
