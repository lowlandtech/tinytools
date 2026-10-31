using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Api.Contracts;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Api.Endpoints;

/// <summary>The JSON body of a render request.</summary>
public sealed record RenderRequest(string Template, string Json);

public static class TemplateEndpoints
{
    public static void MapTemplateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/templates")
            .RequireAuthorization(ApiKeyPolicy.Name)
            .WithTags("Templates");

        group.MapGet("/", async (ITemplateLibrary service, HttpContext context, CancellationToken ct) =>
            {
                TemplateConditions.DeclareUncacheable(context);
                return Results.Ok((await service.ListAsync(ct)).Select(template => template.ToResponse()));
            })
            .WithName("ListTemplates")
            .WithDescription("List every template with its versions and packages.");

        group.MapGet("/{id:guid}", async (Guid id, ITemplateLibrary service, HttpContext context, CancellationToken ct) =>
            {
                var template = await service.GetAsync(id, ct);
                if (template is null) return Results.NotFound();

                var response = template.ToResponse();
                TemplateConditions.DeclareUncacheable(context);
                context.Response.Headers.ETag = "\"" + TemplateConditions.Fingerprint(response) + "\"";
                return Results.Ok(response);
            })
            .WithName("GetTemplate")
            .WithDescription("Get one template, its versions and its packages.");

        group.MapPost("/", async (Template template, ITemplateLibrary service, IdempotencyStore idempotency,
                HttpContext context, CancellationToken ct) =>
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
            })
            .WithName("CreateTemplate")
            .WithDescription("Create a template with its versions.");

        group.MapPut("/{id:guid}", async (Guid id, Template template, ITemplateLibrary service,
                LowlandTech.TinyTools.Api.Domain.TinyToolsDbContext database, HttpContext context, CancellationToken ct) =>
            {
                TemplateConditions.DeclareUncacheable(context);
                // Read the precondition without tracking: the update below attaches its own instance of
                // the same row, and a tracked copy would make EF refuse the second one.
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
            })
            .WithName("UpdateTemplate")
            .WithDescription("Replace a template.");

        group.MapDelete("/{id:guid}", async (Guid id, ITemplateLibrary service, HttpContext context, CancellationToken ct) =>
            {
                TemplateConditions.DeclareUncacheable(context);
                return await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteTemplate")
            .WithDescription("Delete a template with its versions.");

        group.MapPost("/render", (RenderRequest request, ITemplateExecutor executor, HttpContext context) =>
            {
                TemplateConditions.DeclareUncacheable(context);
                return Results.Ok(new { output = executor.Render(request.Template, request.Json) });
            })
            .WithName("RenderTemplate")
            .WithDescription("Render a template with a JSON model using ${Context.xxx} and ${name} syntax.");

        group.MapPost("/versions/{id:guid}/validate", async (Guid id, ITemplateLibrary service, HttpContext context,
                CancellationToken ct) =>
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
            })
            .WithName("ValidateTemplateVersion")
            .WithDescription("Render a version with its test JSON and compare it with its expected text.");
    }
}
