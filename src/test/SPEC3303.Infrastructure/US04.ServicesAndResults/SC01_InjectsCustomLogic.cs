using Humanizer;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

[Trait("SPEC", "3303"), Trait("US", "04"), Trait("SC", "01")]
[UserStory("04", "Template services inject custom logic")]
public sealed class SC01_InjectsCustomLogic : IClassFixture<UseCases.ServicesUseCase>
{
    private readonly UseCases.ServicesUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext context = null!;
    private TinyTemplateEngine engine = null!;

    public SC01_InjectsCustomLogic(UseCases.ServicesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a context holding an entity and a collection name served by a fresh engine")]
    private void Given()
    {
        context = new global::LowlandTech.TinyTools.ExecutionContext();
        context.Set("EntityName", "customer");
        context.Set("CollectionName", "categories");
        engine = new TinyTemplateEngine();
    }

    [When("each registered service renders its own template once")]
    private void When()
    {
        context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        useCase.Cases["Pluralize"] = engine.Render("Entities: ${Context.Services('pluralize')(Context.EntityName)}", context);
        context.RegisterService("singularize", input => input?.ToString()?.Singularize() ?? "");
        useCase.Cases["Singularize"] = engine.Render("Entity: ${Context.Services('singularize')(Context.CollectionName)}", context);
        useCase.Cases["Irregular"] = engine.Render("${Context.Services('pluralize')('person')}, ${Context.Services('pluralize')('child')}, ${Context.Services('pluralize')('goose')}", context);
        useCase.Cases["Missing"] = engine.Render("${Context.Services('nonexistent')('test')}", context);
    }

    [Then("the pluralized entities should contain customers")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PluralizesAnEntity() => ((string)useCase.Cases["Pluralize"]!).Should().Contain("Entities: customers");

    [Then("the singularized entity should contain category")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_SingularizesACollection() => ((string)useCase.Cases["Singularize"]!).Should().Contain("Entity: category");

    [Then("the irregular person should contain people")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PluralizesPerson() => ((string)useCase.Cases["Irregular"]!).Should().Contain("people");

    [Then("the irregular child should contain children")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_PluralizesChild() => ((string)useCase.Cases["Irregular"]!).Should().Contain("children");

    [Then("the irregular goose should contain geese")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PluralizesGoose() => ((string)useCase.Cases["Irregular"]!).Should().Contain("geese");

    [Then("the missing service should contain {nonexistent not registered}")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_AnswersForAMissingService() => ((string)useCase.Cases["Missing"]!).Should().Contain("{nonexistent not registered}");
}
