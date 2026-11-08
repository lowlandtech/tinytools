using System.Text.Json;
using LowlandTech.Core.Plugins;
using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Api.Contracts;
using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>The JSON body of a render request.</summary>
public sealed record RenderRequest(string Template, string Json);

/// <summary>The one resource surface, mapped by both the standalone host and a composed host.</summary>
public static class TemplateEndpoints
{
    /// <summary>Maps the resource-based API in the standalone development host.</summary>
    public static void MapTemplateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/templates")
            .RequireAuthorization(ApiKeyPolicy.Name)
            .WithTags("Templates");

        MapTemplateEndpoints(group);
    }

    /// <summary>Maps the endpoint handlers into the standalone API route group.</summary>
    public static void MapTemplateEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ListTemplates)
            .WithName("ListTemplates")
            .WithDescription("List every template with its versions and packages.");
        group.MapGet("/{id:guid}", GetTemplate)
            .WithName("GetTemplate")
            .WithDescription("Get one template, its versions and its packages.");
        group.MapPost("/", CreateTemplate)
            .WithName("CreateTemplate")
            .WithDescription("Create a template with its versions.");
        group.MapPut("/{id:guid}", UpdateTemplate)
            .WithName("UpdateTemplate")
            .WithDescription("Replace a template.");
        group.MapDelete("/{id:guid}", DeleteTemplate)
            .WithName("DeleteTemplate")
            .WithDescription("Delete a template with its versions.");
        group.MapPost("/render", RenderTemplate)
            .WithName("RenderTemplate")
            .WithDescription("Render a template with a JSON model using ${Context.xxx} and ${name} syntax.");
        group.MapPost("/versions/{id:guid}/validate", ValidateTemplateVersion)
            .WithName("ValidateTemplateVersion")
            .WithDescription("Render a version with its test JSON and compare it with its expected text.");
    }

    /// <summary>Maps the same handlers through the host-owned portable endpoint-group contract.</summary>
    public static void MapTemplateEndpoints(this IEndpointGroupContext context)
    {
        context.Map("GET", "/", (Func<ITemplateLibrary, HttpContext, CancellationToken, Task<IResult>>)ListTemplates);
        context.Map("GET", "/{id:guid}", (Func<Guid, ITemplateLibrary, HttpContext, CancellationToken, Task<IResult>>)GetTemplate);
        context.Map("POST", "/", (Func<Template, ITemplateLibrary, IdempotencyStore, HttpContext, CancellationToken, Task<IResult>>)CreateTemplate);
        context.Map("PUT", "/{id:guid}", (Func<Guid, Template, ITemplateLibrary, TinyToolsDbContext, HttpContext, CancellationToken, Task<IResult>>)UpdateTemplate);
        context.Map("DELETE", "/{id:guid}", (Func<Guid, ITemplateLibrary, HttpContext, CancellationToken, Task<IResult>>)DeleteTemplate);
        context.Map("POST", "/render", (Func<RenderRequest, ITemplateExecutor, HttpContext, IResult>)RenderTemplate);
        context.Map("POST", "/versions/{id:guid}/validate", (Func<Guid, ITemplateLibrary, HttpContext, CancellationToken, Task<IResult>>)ValidateTemplateVersion);
    }

    private static async Task<IResult> ListTemplates(
        ITemplateLibrary service, HttpContext context, CancellationToken ct)
    {
        TemplateConditions.DeclareUncacheable(context);
        return Results.Ok((await service.ListAsync(ct)).Select(template => template.ToResponse()));
    }

    private static async Task<IResult> GetTemplate(
        Guid id, ITemplateLibrary service, HttpContext context, CancellationToken ct)
    {
        var template = await service.GetAsync(id, ct);
        if (template is null) return Results.NotFound();

        var response = template.ToResponse();
        TemplateConditions.DeclareUncacheable(context);
        context.Response.Headers.ETag = "\"" + TemplateConditions.Fingerprint(response) + "\"";
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateTemplate(
        Template template, ITemplateLibrary service, IdempotencyStore idempotency,
        HttpContext context, CancellationToken ct)
    {
        TemplateConditions.DeclareUncacheable(context);
        var key = context.Request.Headers[TemplateConditions.IdempotencyHeader].ToString();
        var fingerprint = TemplateConditions.Fingerprint(new { template.Name, template.Versions });

        if (!string.IsNullOrWhiteSpace(key))
        {
            if (idempotency.TryReplay(key, fingerprint, out var replayed, out var reused))
            {
                var existing = await service.GetAsync(replayed, ct);
                if (existing is not null) return Results.Ok(existing.ToResponse());
            }

            if (reused)
            {
                return Results.Problem(
                    type: TemplateConditions.IdempotencyReused,
                    title: "Idempotency key reused",
                    detail: "The key was already used for a different payload.",
                    statusCode: StatusCodes.Status409Conflict);
            }
        }

        var created = await service.CreateAsync(template, ct);
        if (!string.IsNullOrWhiteSpace(key)) idempotency.Record(key, fingerprint, created.Id);
        return Results.Created($"/api/templates/{created.Id}", created.ToResponse());
    }

    private static async Task<IResult> UpdateTemplate(
        Guid id, Template template, ITemplateLibrary service, TinyToolsDbContext database,
        HttpContext context, CancellationToken ct)
    {
        TemplateConditions.DeclareUncacheable(context);
        var current = await database.Templates.AsNoTracking()
            .Include(row => row.Versions).Include(row => row.Packages)
            .FirstOrDefaultAsync(row => row.Id == id, ct);
        if (current is null) return Results.NotFound();

        var ifMatch = context.Request.Headers.IfMatch.ToString();
        var etag = "\"" + TemplateConditions.Fingerprint(current.ToResponse()) + "\"";
        if (!string.IsNullOrWhiteSpace(ifMatch) && !string.Equals(ifMatch.Trim(), etag, StringComparison.Ordinal))
        {
            return Results.Problem(
                type: TemplateConditions.StaleTemplate,
                title: "The template changed",
                detail: "The supplied ETag does not match the stored template.",
                statusCode: StatusCodes.Status412PreconditionFailed);
        }

        template.Id = id;
        var updated = await service.UpdateAsync(template, ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated.ToResponse());
    }

    private static async Task<IResult> DeleteTemplate(
        Guid id, ITemplateLibrary service, HttpContext context, CancellationToken ct)
    {
        TemplateConditions.DeclareUncacheable(context);
        return await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound();
    }

    private static IResult RenderTemplate(RenderRequest request, ITemplateExecutor executor, HttpContext context)
    {
        TemplateConditions.DeclareUncacheable(context);
        var exceeded = TemplateBounds.Exceeded(request.Template, request.Json);
        if (exceeded is not null)
        {
            return Results.Problem(
                type: TemplateBounds.TooLarge,
                title: "The payload is too large",
                detail: $"The {exceeded} exceeds the bound of this operation.",
                statusCode: StatusCodes.Status413PayloadTooLarge);
        }

        return Results.Ok(new { output = executor.Render(request.Template, request.Json) });
    }

    private static async Task<IResult> ValidateTemplateVersion(
        Guid id, ITemplateLibrary service, HttpContext context, CancellationToken ct)
    {
        TemplateConditions.DeclareUncacheable(context);
        try
        {
            var validation = await service.ValidateVersionAsync(id, ct);
            return validation is null ? Results.NotFound() : Results.Ok(validation);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Problem(
                title: "The version is not validatable",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
