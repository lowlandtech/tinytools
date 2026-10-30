using LowlandTech.TinyTools.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Api.Domain;

/// <summary>
/// The template library of the server. The imported entities carry no mapping,
/// so the relationship and schema contract lives here, beside the host that
/// owns the database.
/// </summary>
public sealed class TinyToolsDbContext(DbContextOptions<TinyToolsDbContext> options) : DbContext(options)
{
    public const string Schema = "tinytools";

    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<TemplatePackage> Packages => Set<TemplatePackage>();
    public DbSet<TemplateImport> Imports => Set<TemplateImport>();
    public DbSet<StepIntent> StepIntents => Set<StepIntent>();
    public DbSet<TemplateFlow> TemplateFlows => Set<TemplateFlow>();
    public DbSet<TemplateStep> TemplateSteps => Set<TemplateStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<Template>()
            .HasMany(template => template.Versions)
            .WithOne(version => version.Template)
            .HasForeignKey(version => version.TemplateId);

        modelBuilder.Entity<Template>()
            .HasMany(template => template.Packages)
            .WithMany(package => package.Templates);

        modelBuilder.Entity<TemplatePackage>()
            .HasMany(package => package.Imports)
            .WithOne(import => import.Package)
            .HasForeignKey(import => import.PackageId);

        modelBuilder.Entity<StepIntent>()
            .HasMany(intent => intent.Steps)
            .WithOne(step => step.StepIntent)
            .HasForeignKey(step => step.StepIntentId);

        modelBuilder.Entity<TemplateFlow>()
            .HasMany(flow => flow.Steps)
            .WithOne(step => step.Flow)
            .HasForeignKey(step => step.FlowId);
    }
}
