# Running Template System Tests

## Quick Start

Run all template system tests:

```bash
dotnet test --filter "FullyQualifiedName~WhenValidatingComponentTemplate | FullyQualifiedName~WhenValidatingCSharpClassTemplate | FullyQualifiedName~WhenUsingTemplateRegistry | FullyQualifiedName~WhenUsingTemplateBase"
```

Or run all tests:

```bash
dotnet test
```

## Test Categories

### 1. ComponentTemplate Tests

Test the React/TypeScript component template:

```bash
dotnet test --filter "FullyQualifiedName~WhenValidatingComponentTemplate"
```

**What it tests:**
- Self-validation with embedded test data
- Rendering with custom data
- Props interface generation
- Component naming conventions
- Edge cases (empty props)

**Example output:**
```
? ItShouldPassValidation
? ItShouldGenerateCorrectPath
? ItShouldContainComponentName
... 22 more tests
```

### 2. CSharpClassTemplate Tests

Test the C# class file generator:

```bash
dotnet test --filter "FullyQualifiedName~WhenValidatingCSharpClassTemplate"
```

**What it tests:**
- Class generation with properties/methods
- Namespace-based path generation
- Constructor inclusion
- Default values
- Multiple members

**Example output:**
```
? ItShouldPassValidation
? ItShouldGenerateNestedPath
? ItShouldContainAllProperties
... 27 more tests
```

### 3. TemplateRegistry Tests

Test template management and batch operations:

```bash
dotnet test --filter "FullyQualifiedName~WhenUsingTemplateRegistry"
```

**What it tests:**
- Template registration/retrieval
- Validation of all templates
- Rendering by name
- Batch operations
- Error handling

**Example output:**
```
? ItShouldRetrieveComponentTemplate
? ItShouldValidateAllTemplates
? ItShouldRenderByName
... 22 more tests
```

### 4. TemplateBase Tests

Test base functionality and edge cases:

```bash
dotnet test --filter "FullyQualifiedName~WhenUsingTemplateBase"
```

**What it tests:**
- Data normalization
- JSON deserialization
- Validation results
- Metadata support
- Custom engines

**Example output:**
```
? ItShouldNormalizeAnonymousDataSuccessfully
? ItShouldCreateSuccessResult
? ItShouldHaveMetadata
... 12 more tests
```

## Watching Tests

Run tests in watch mode during development:

```bash
dotnet watch test
```

Changes to template code will automatically trigger test runs.

## Test Coverage

Generate coverage report:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

View coverage:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

Then open `coverage-report/index.html` in your browser.

## Debugging Tests

### Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Right-click test ? Debug
3. Set breakpoints in template code

### VS Code
1. Install .NET Test Explorer extension
2. Click debug icon next to test
3. Set breakpoints as needed

### Command Line
```bash
dotnet test --filter "FullyQualifiedName~WhenValidatingComponentTemplate.ItShouldPassValidation" --logger "console;verbosity=detailed"
```

## Writing New Tests

### Pattern to Follow

```csharp
using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

public class WhenTestingMyNewTemplate : WhenTestingFor<MyNewTemplate>
{
    private TemplateResult? _result;
    
    protected override MyNewTemplate For()
    {
        return new MyNewTemplate(); // Create your template
    }
    
    protected override void Given()
    {
        // Setup test data if needed
    }
    
    protected override void When()
    {
        // Execute the operation
        _result = Sut.Render(testData);
    }
    
    [Fact]
    public void ItShouldDoSomething()
    {
        // Assert expectations
        _result.Should().NotBeNull();
    }
}
```

### Best Practices

1. **One concept per test class**
   - `WhenValidatingX` for validation tests
   - `WhenRenderingXWithY` for rendering tests
   - `WhenUsingXForY` for functionality tests

2. **Descriptive test names**
   - ? `ItShouldGenerateCorrectPath`
   - ? `ItShouldContainAllProperties`
   - ? `TestPath`
   - ? `Test1`

3. **Use FluentAssertions**
   ```csharp
   // ? Good
   result.Should().NotBeNull();
   result!.Content.Should().Contain("expected");
   
   // ? Avoid
   Assert.NotNull(result);
   Assert.Contains("expected", result.Content);
   ```

4. **Test self-validation**
   ```csharp
   [Fact]
   public void ItShouldPassValidation()
   {
       var template = new MyTemplate();
       template.Validate().Should().BeTrue();
   }
   ```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Run Tests
        run: dotnet test --logger "trx;LogFileName=test-results.trx"
      - name: Publish Test Results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: Test Results
          path: '**/test-results.trx'
          reporter: dotnet-trx
```

## Troubleshooting

### Tests not discovered
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test --list-tests
```

### Flaky tests
- Check for timing issues
- Verify test isolation
- Use `--blame` flag to identify culprit

### Test timeout
```bash
# Increase timeout
dotnet test -- NUnit.Timeout=60000
```

## Performance Testing

For large-scale generation (500+ files):

```csharp
[Fact]
public void ItShouldGenerateMultipleFilesQuickly()
{
    var sw = Stopwatch.StartNew();
    
    for (int i = 0; i < 100; i++)
    {
        var result = template.Render(data);
    }
    
    sw.Stop();
    sw.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 second for 100 files
}
```

## Summary

| Command | Purpose |
|---------|---------|
| `dotnet test` | Run all tests |
| `dotnet watch test` | Watch mode |
| `dotnet test --filter "X"` | Run specific tests |
| `dotnet test --collect:"XPlat Code Coverage"` | Coverage |
| `dotnet test --logger trx` | TRX output for CI |

**Total Tests: ~95**
- ComponentTemplate: 25 tests
- CSharpClassTemplate: 30 tests
- TemplateRegistry: 25 tests
- TemplateBase: 15 tests

All following the `WhenTestingFor` pattern for consistency and clarity.
