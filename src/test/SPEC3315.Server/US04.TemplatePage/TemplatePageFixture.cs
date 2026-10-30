#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US04.TemplatePage;

/// <summary>The template set the page scenario renders, kept out of the scenario body.</summary>
internal static class TemplatePageFixture
{
    public static Template[] TwoTemplates() =>
    [
        new() { Id = Guid.NewGuid(), Name = "EntityClass", IsActive = true },
        new() { Id = Guid.NewGuid(), Name = "DbContextClass", IsActive = false },
    ];
}
#endif
