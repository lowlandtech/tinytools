using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "07")]
[UserStory("02", "Template engine exposes loop metadata and nests control flow")]
public sealed class SC07_ExposesLoopMetadataAndNestedFlow : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _people = string.Empty;
    private string _indexes = string.Empty;
    private string _firstLast = string.Empty;
    private string _endSyntax = string.Empty;
    private string _counts = string.Empty;
    private string _singleItem = string.Empty;
    private string _nestedIf = string.Empty;
    private string _ifInsideForeach = string.Empty;
    private string _foreachInsideIf = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each loop metadata and nested control-flow template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("People", new[] { new { Name = "John", Age = 30 }, new { Name = "Jane", Age = 25 } });
        _people = Sut.Render("@foreach(var person in Context.People) {\n${person.Name} is ${person.Age}\n}", _context);
        _context.Set("Items", new[] { "A", "B", "C" });
        _indexes = Sut.Render("@foreach(var item in Context.Items) {\n${_index}:${item}\n}", _context);
        _firstLast = Sut.Render(
            """
            @foreach(var item in Context.Items) {
            @if(_first) {
            First: ${item}
            } else if (_last) {
            Last: ${item}
            } else {
            Middle: ${item}
            }
            }
            """, _context);
        _endSyntax = Sut.Render("@foreach(var item in Context.Items)\n@if(_first)\nGiven ${item}\n@end\n@if(!_first)\nAnd ${item}\n@end\n@end", _context);
        _context.Set("Items", new[] { "X", "Y" });
        _counts = Sut.Render("@foreach(var item in Context.Items) {\n${item} (${_count} total)\n}", _context);
        _context.Set("Items", new[] { "Only" });
        _singleItem = Sut.Render("@foreach(var item in Context.Items) {\n@if(_first) {\nfirst\n}\n@if(_last) {\nlast\n}\n}", _context);
        _context.Set("IsActive", true);
        _context.Set("IsPremium", true);
        _nestedIf = Sut.Render("@if(Context.IsActive) {\n@if(Context.IsPremium) {\nPremium Active User\n}\n}", _context);
        _context.Set("Numbers", new[] { 1, 2, 3, 4, 5 });
        _ifInsideForeach = Sut.Render("@foreach(var num in Context.Numbers) {\n@if(num > 3) {\n${num} is greater than 3\n}\n}", _context);
        _context.Set("HasItems", true);
        _context.Set("Items", new[] { "A", "B", "C" });
        _foreachInsideIf = Sut.Render("@if(Context.HasItems) {\n@foreach(var item in Context.Items) {\nItem: ${item}\n}\n}", _context);
    }

    public SC07_ExposesLoopMetadataAndNestedFlow() => ArrangeAndAct();

    [Then("the iterated people should contain both people")]
    [Trait("UAC", "28"), Fact]
    public void UAC28_AccessesItemPropertiesInForeach() => _people.Should().Contain("John is 30").And.Contain("Jane is 25");

    [Then("the indexed loop should contain every index")]
    [Trait("UAC", "29"), Fact]
    public void UAC29_ExposesIndexMetadataInForeach() => _indexes.Should().Contain("0:A").And.Contain("1:B").And.Contain("2:C");

    [Then("the first and last loop should contain every marker")]
    [Trait("UAC", "30"), Fact]
    public void UAC30_ExposesFirstAndLastMetadataInForeach() => _firstLast.Should().Contain("First: A").And.Contain("Middle: B").And.Contain("Last: C");

    [Then("the counted loop should contain both totals")]
    [Trait("UAC", "31"), Fact]
    public void UAC31_ExposesCountMetadataInForeach() => _counts.Should().Contain("X (2 total)").And.Contain("Y (2 total)");

    [Then("the single item loop should contain first and last")]
    [Trait("UAC", "32"), Fact]
    public void UAC32_ExposesFirstLastForSingleItemCollection() => _singleItem.Should().Contain("first").And.Contain("last");

    [Then("the end syntax loop should contain every line")]
    [Trait("UAC", "33"), Fact]
    public void UAC33_ExposesLoopMetadataWithEndSyntax() => _endSyntax.Should().Contain("Given A").And.Contain("And B").And.Contain("And C").And.NotContain("Given B").And.NotContain("Given C");

    [Then("the nested if blocks should contain Premium Active User")]
    [Trait("UAC", "34"), Fact]
    public void UAC34_HandlesNestedIfStatements() => _nestedIf.Should().Contain("Premium Active User");

    [Then("the if inside the loop should contain the large numbers")]
    [Trait("UAC", "35"), Fact]
    public void UAC35_HandlesIfInsideForeach() => _ifInsideForeach.Should().Contain("4 is greater than 3").And.Contain("5 is greater than 3").And.NotContain("1 is greater");

    [Then("the loop inside the if should contain every line")]
    [Trait("UAC", "36"), Fact]
    public void UAC36_HandlesForeachInsideIf() => _foreachInsideIf.Should().Contain("Item: A").And.Contain("Item: B").And.Contain("Item: C");
}
