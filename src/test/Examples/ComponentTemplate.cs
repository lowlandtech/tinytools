namespace LowlandTech.TinyTools.UnitTests.Examples;

/// <summary>
/// Example template for generating React-like component files.
/// Demonstrates how to create a template for massive-scale code generation.
/// </summary>
public class ComponentTemplate : TemplateBase
{
    public override string TemplatePath => "src/components/${Context.ComponentName}.tsx";

    public override string TemplateNamespace => "App.Components";

    public override string TemplateContent => 
@"import React from 'react';

export interface ${Context.ComponentName}Props {
@foreach (var prop in Context.Props) {
  ${prop.Name}: ${prop.Type};
}
}

export const ${Context.ComponentName}: React.FC<${Context.ComponentName}Props> = (${Context.PropsDestructured}) => {
  return (
    <div className=""${Context.ComponentName | lower}"">
@foreach (var prop in Context.Props) {
      <div>{${prop.Name}}</div>
}
    </div>
  );
};
";

    public override Type DataType => typeof(ComponentData);

    public override string TestDataJson => 
@"{
  ""ComponentName"": ""UserCard"",
  ""Props"": [
    { ""Name"": ""name"", ""Type"": ""string"" },
    { ""Name"": ""age"", ""Type"": ""number"" }
  ],
  ""PropsDestructured"": ""{ name, age }""
}";

    public override string ExpectedContent => 
@"import React from 'react';

export interface UserCardProps {
  name: string;
  age: number;
}

export const UserCard: React.FC<UserCardProps> = ({ name, age }) => {
  return (
    <div className=""usercard"">
      <div>{name}</div>
      <div>{age}</div>
    </div>
  );
};
";

    public override string ExpectedPath => "src/components/UserCard.tsx";

    public override string ExpectedNamespace => "App.Components";
}

/// <summary>
/// Data structure for component generation.
/// </summary>
public record ComponentData
{
    public string ComponentName { get; init; } = string.Empty;
    public List<PropDefinition> Props { get; init; } = new();
    public string PropsDestructured { get; init; } = string.Empty;
}

public record PropDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
