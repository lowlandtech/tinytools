using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Api.Contracts;

/// <summary>
/// The wire shape of a template. The entities carry navigations in both
/// directions, so returning them directly would serialize a cycle; the response
/// names the fields the contract owns and drops the back-references.
/// </summary>
public sealed record TemplateResponse(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyList<TemplateVersionResponse> Versions,
    IReadOnlyList<TemplatePackageResponse> Packages);

public sealed record TemplateVersionResponse(
    Guid Id,
    string Name,
    bool IsActive,
    string Content,
    string Hash,
    string DataType,
    string? Json,
    string? Text,
    bool? IsValid,
    Guid TemplateId);

public sealed record TemplatePackageResponse(
    Guid Id,
    string Name,
    bool IsActive,
    string PackageVersion,
    IReadOnlyList<TemplateImportResponse> Imports);

public sealed record TemplateImportResponse(Guid Id, string Name, bool IsActive, Guid PackageId);

public static class TemplateContracts
{
    public static TemplateResponse ToResponse(this Template template) => new(
        template.Id,
        template.Name,
        template.IsActive,
        template.Versions
            .OrderBy(version => version.Name, StringComparer.Ordinal)
            .Select(version => version.ToResponse())
            .ToList(),
        template.Packages.Select(package => package.ToResponse()).ToList());

    public static TemplateVersionResponse ToResponse(this TemplateVersion version) => new(
        version.Id,
        version.Name,
        version.IsActive,
        version.Content,
        version.Hash,
        version.DataType,
        version.Json,
        version.Text,
        version.IsValid,
        version.TemplateId);

    public static TemplatePackageResponse ToResponse(this TemplatePackage package) => new(
        package.Id,
        package.Name,
        package.IsActive,
        package.PackageVersion,
        package.Imports.Select(import => import.ToResponse()).ToList());

    public static TemplateImportResponse ToResponse(this TemplateImport import) => new(
        import.Id,
        import.Name,
        import.IsActive,
        import.PackageId);
}
