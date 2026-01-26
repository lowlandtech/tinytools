# Helpers and Pipes

Helpers transform values inside `${...}` expressions using pipe syntax.

```
${Context.Name | upper}
${Context.Price | format:N2}
${Context.Tags | join:, }
```

Helpers can be chained:

```
${Context.Name | trim | upper}
${Context.Description | truncate:30}
```

## String Helpers

```csharp
var template = """
    Upper: ${Context.Name | upper}
    Lower: ${Context.Name | lower}
    Capitalize: ${Context.Name | capitalize}
    Trim: [${Context.Title | trim}]
    Truncate: ${Context.Description | truncate:30}
    Replace: ${Context.Path | replace:old,new}
    CamelCase: ${Context.ClassName | camelcase}
    PascalCase: ${Context.ClassName | pascalcase}
    """;
```

## Date Helpers

```csharp
var template = """
    ISO Date: ${Context.OrderDate | format:yyyy-MM-dd}
    US Date: ${Context.OrderDate | format:MM/dd/yyyy}
    Time: ${Context.OrderDate | format:HH:mm}
    Default Date: ${Context.BirthDate | date}
    Custom Date: ${Context.BirthDate | date:dd-MMM-yyyy}
    """;
```

## Number Helpers

```csharp
var template = """
    Formatted Number: ${Context.Quantity | number}
    Two Decimals: ${Context.Price | format:N2}
    Percentage: ${Context.Percentage | format:P0}
    Round 2: ${Context.Pi | round:2}
    Floor: ${Context.Price | floor}
    Ceiling: ${Context.Price | ceiling}
    """;
```

## Collection Helpers

```csharp
var template = """
    Count: ${Context.Tags | count}
    First: ${Context.Tags | first}
    Last: ${Context.Tags | last}
    Join: ${Context.Tags | join:, }
    Join Dash: ${Context.Numbers | join:-}
    Reverse String: ${Context.Word | reverse}
    """;
```

## Conditional Helpers

```csharp
var template = """
    Name: ${Context.Name | default:Unknown}
    Nickname: ${Context.Nickname | default:No nickname}
    Empty: ${Context.EmptyValue | ifempty:N/A}
    Active: ${Context.IsActive | yesno}
    Custom Yes/No: ${Context.IsActive | yesno:Enabled,Disabled}
    Count Status: ${Context.Count | yesno:Has items,Empty}
    """;
```
