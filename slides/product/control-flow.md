# Control Flow

TinyTemplateEngine supports structured control flow inside templates while keeping the syntax compact and readable.

## If / Else If / Else

```csharp
var template = """
    Grade:
    @if (Context.Score >= 90) {
    A - Excellent!
    } else if (Context.Score >= 80) {
    B - Good job!
    } else if (Context.Score >= 70) {
    C - Satisfactory
    } else {
    F - Failed
    }
    """;
```

## Comparison Operators

All standard comparisons are supported.

```csharp
var template = """
    @if (Context.Age >= 21) {
    You can purchase alcohol.
    }
    @if (Context.Balance <= 100) {
    Low balance warning.
    }
    @if (Context.ItemCount > 3) {
    Bulk discount applied!
    }
    """;
```

## Negation

```csharp
var template = """
    @if (!Context.IsActive) {
    Account is inactive.
    }
    @if (!(Context.Count > 0)) {
    No items in cart.
    }
    """;
```

## Foreach Loops

```csharp
var template = """
    @foreach (var item in Context.Items) {
    - ${item}
    }
    """;
```

## Comments

Comments are stripped from the output.

```csharp
var template = """
    @* This is a single-line comment *@
    Hello, ${Context.Name}!
    @*
    This is a
    multi-line comment
    *@
    Welcome to our platform.
    """;
```
