using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Api.Contracts;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>The JSON body of a render request.</summary>
public sealed record RenderRequest(string Template, string Json);

public static class TemplateEndpoints
{
    public static void MapTemplateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tinytools/templates")
            .RequireAuthorization(ApiKeyPolicy.Name)
            .WithTags("Templates");

        group.MapGet("/", async (ITemplateLibrary service, CancellationToken ct) =>
                Results.Ok((await service.ListAsync(ct)).Select(template => template.ToResponse())))
            .WithName("ListTemplates")
            .WithDescription("List every template with its versions and packages.");

        group.MapGet("/{id:guid}", async (Guid id, ITemplateLibrary service, CancellationToken ct) =>
            {
                var template = await service.GetAsync(id, ct);
                return template is null ? Results.NotFound() : Results.Ok(template.ToResponse());
            })
            .WithName("GetTemplate")
            .WithDescription("Get one template, its versions and its packages.");

        group.MapPost("/", async (Template template, ITemplateLibrary service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(template, ct);
                return Results.Created($"/api/tinytools/templates/{created.Id}", created.ToResponse());
            })
            .WithName("CreateTemplate")
            .WithDescription("Create a template with its versions.");

        group.MapPut("/{id:guid}", async (Guid id, Template template, ITemplateLibrary service, CancellationToken ct) =>
            {
                template.Id = id;
                var updated = await service.UpdateAsync(template, ct);
                return updated is null ? Results.NotFound() : Results.Ok(updated.ToResponse());
            })
            .WithName("UpdateTemplate")
            .WithDescription("Replace a template.");

        group.MapDelete("/{id:guid}", async (Guid id, ITemplateLibrary service, CancellationToken ct) =>
                await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteTemplate")
            .WithDescription("Delete a template with its versions.");

        group.MapPost("/render", (RenderRequest request, ITemplateExecutor executor) =>
                Results.Ok(new { output = executor.Render(request.Template, request.Json) }))
            .WithName("RenderTemplate")
            .WithDescription("Render a template with a JSON model using ${Context.xxx} and ${name} syntax.");

        group.MapPost("/versions/{id:guid}/validate", async (Guid id, ITemplateLibrary service, CancellationToken ct) =>
            {
                try
                {
                    var validation = await service.ValidateVersionAsync(id, ct);
                    return validation is null ? Results.NotFound() : Results.Ok(validation);
                }
                catch (InvalidOperationException exception)
                {
                    return Results.BadRequest(new { error = exception.Message });
                }
            })
            .WithName("ValidateTemplateVersion")
            .WithDescription("Render a version with its test JSON and compare it with its expected text.");
    }
}
