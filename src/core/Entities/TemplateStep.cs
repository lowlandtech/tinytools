namespace LowlandTech.TinyTools.Core.Entities;

public class TemplateStep
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>Keyed service ID for the IFlowStep handler.</summary>
    public Guid TypeId { get; set; }

    /// <summary>Keyed service ID for the IFlowDataContext — null means run once (no iteration).</summary>
    public Guid? DataSourceId { get; set; }

    /// <summary>Entity type to query when DataSourceId is set, e.g. "Product", "Block".</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Dynamic LINQ filter expression applied to the data source query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>NCalc expression evaluated before executing; empty means always run.</summary>
    public string Conditions { get; set; } = string.Empty;

    public Guid FlowId { get; set; }
    public Guid StepIntentId { get; set; }

    public TemplateFlow Flow { get; set; } = null!;
    public StepIntent StepIntent { get; set; } = null!;
}
