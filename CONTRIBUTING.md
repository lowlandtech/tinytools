# Contributing to TinyTemplateEngine

Thank you for your interest in contributing to TinyTemplateEngine! We welcome contributions from the community.

## 🎯 Project Philosophy

Before contributing, please understand our core principles:

1. **Keep it Tiny** - The core library stays minimal with zero unnecessary dependencies
2. **Data-First** - Templates are projections of data, not views or applications
3. **Explicit over Implicit** - Clear, predictable behavior without magic
4. **Extensibility through Services** - Complex features via opt-in Template Services

## 🚀 Getting Started

### Prerequisites

- .NET SDK 8.0, 9.0, or 10.0
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Setup

```bash
# Clone the repository
git clone https://github.com/lowlandtech/tinytools.git
cd tinytools

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## 📝 How to Contribute

### Reporting Bugs

1. **Check existing issues** - Someone may have already reported it
2. **Use the issue template** - Provide as much detail as possible
3. **Include reproduction steps** - Help us reproduce the issue
4. **Add code samples** - Minimal, complete examples are best

### Suggesting Features

1. **Open a discussion first** - Discuss your idea before coding
2. **Align with philosophy** - Ensure it fits the "tiny" principle
3. **Consider Template Services** - Can it be a service instead of core feature?
4. **Provide use cases** - Real-world examples help

### Pull Request Process

1. **Fork the repository**
2. **Create a feature branch** - `git checkout -b feature/amazing-feature`
3. **Make your changes**
   - Follow existing code style
   - Add/update tests
   - Update documentation
4. **Commit your changes** - Use clear, descriptive commit messages
5. **Push to your fork** - `git push origin feature/amazing-feature`
6. **Open a Pull Request** - Fill in the PR template

### Code Style

- **C# 12 features** - Use modern C# idioms
- **Nullable reference types** - Always enabled
- **XML documentation** - Public APIs must be documented
- **Keep methods focused** - Single responsibility principle
- **Avoid complexity** - Prefer clarity over cleverness

### Testing Guidelines

- **Test coverage** - All new features must have tests
- **Use descriptive names** - `ItShouldPluralizeUsingHumanizerService`
- **Follow AAA pattern** - Arrange, Act, Assert
- **One assertion per test** - Keep tests focused
- **Test edge cases** - Null, empty, invalid inputs

Example test:
```csharp
[Fact]
public void ItShouldHandleNullInput()
{
    // Arrange
    var context = new ExecutionContext();
    context.RegisterService("test", input => input?.ToString() ?? "default");
    
    // Act
    var result = engine.Render("${Context.Services('test')(null)}", context);
    
    // Assert
    result.Should().Contain("default");
}
```

## 🏗️ Project Structure

```
tinytools/
├── src/
│   └── lowlandtech.tinytools/      # Core library
│       ├── TinyTemplateEngine.cs   # Main engine
│       ├── ExecutionContext.cs     # Template context
│       ├── VariableResolver.cs     # Expression resolver
│       ├── TemplateHelpers.cs      # Built-in helpers
│       └── ITemplateService.cs     # Service interface
├── test/
│   └── lowlandtech.tinytools.unittests/  # Unit tests
├── samples/
│   └── README.md                   # Service examples
├── .github/
│   └── workflows/                  # CI/CD pipelines
└── readme.md                       # Main documentation
```

## 🔄 Development Workflow

1. **Sync with main** - `git pull origin main`
2. **Create feature branch** - Descriptive name
3. **Write failing test** - TDD approach preferred
4. **Implement feature** - Make test pass
5. **Refactor** - Clean up code
6. **Update docs** - README, samples, XML docs
7. **Push and PR** - Submit for review

## 📚 Documentation

### When to Update Docs

- **New features** - Always document
- **Breaking changes** - Migration guide required
- **Bug fixes** - If behavior changes
- **Examples** - Real-world use cases welcome

### Documentation Locations

- **readme.md** - User-facing documentation
- **XML comments** - API documentation
- **samples/** - Usage examples
- **changelog.md** - Version history

## 🎨 Template Services

When adding Template Services examples:

1. **Keep them simple** - Clear, focused functions
2. **Show both registrations** - Code and usage
3. **Add to samples/README.md** - Comprehensive guide
4. **Include tests** - Working examples

## ⚖️ License

By contributing, you agree that your contributions will be licensed under the MIT License.

## 🤝 Code of Conduct

- Be respectful and inclusive
- Welcome newcomers
- Focus on constructive feedback
- Assume positive intent

## 💬 Getting Help

- **GitHub Discussions** - Ask questions
- **Issues** - Report bugs
- **Twitter** - [@wendellmva](https://twitter.com/wendellmva)

## 🙏 Recognition

Contributors will be acknowledged in:
- Release notes
- README contributors section
- Package metadata (where applicable)

---

Thank you for helping make TinyTemplateEngine better! 🎉
