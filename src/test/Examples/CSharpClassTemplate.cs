namespace LowlandTech.TinyTools.UnitTests.Examples;

/// <summary>
/// Example template for generating C# class files.
/// Shows how to generate .NET code with proper namespaces and paths.
/// </summary>
public class CSharpClassTemplate : TemplateBase
{
    public override string TemplatePath => "src/${Context.NamespacePath}/${Context.ClassName}.cs";

    public override string TemplateNamespace => "${Context.Namespace}";

    public override string TemplateContent => 
@"namespace ${Context.Namespace};

/// <summary>
/// ${Context.Description}
/// </summary>
public class ${Context.ClassName}
{
@foreach (var prop in Context.Properties) {
    /// <summary>
    /// ${prop.Description}
    /// </summary>
    public ${prop.Type} ${prop.Name} { get; set; }${prop.DefaultValue != null ? ' = ' + prop.DefaultValue + ';' : ''}

}
@if (Context.IncludeConstructor) {
    public ${Context.ClassName}()
    {
    }

}
@foreach (var method in Context.Methods) {
    /// <summary>
    /// ${method.Description}
    /// </summary>
    public ${method.ReturnType} ${method.Name}(${method.Parameters})
    {
        throw new NotImplementedException();
    }

}
}
";

    public override Type DataType => typeof(ClassData);

    public override string TestDataJson => """
        {
          "Namespace": "MyApp.Models",
          "ClassName": "User",
          "Description": "Represents a user in the system",
          "IncludeConstructor": true,
          "Properties": [
            { "Name": "Id", "Type": "int", "Description": "User identifier", "DefaultValue": null },
            { "Name": "Name", "Type": "string", "Description": "User name", "DefaultValue": "\"\"" }
          ],
          "Methods": [
            { "Name": "Validate", "ReturnType": "bool", "Parameters": "", "Description": "Validates the user data" }
          ]
        }
        """;

    public override string ExpectedPath => "src/MyApp/Models/User.cs";

    public override string ExpectedNamespace => "MyApp.Models";
    
    public override string ExpectedContent => @"namespace MyApp.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    /// <summary>
    /// User identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public string Name { get; set; } = """";

    public User()
    {
    }

    /// <summary>
    /// Validates the user data
    /// </summary>
    public bool Validate()
    {
        throw new NotImplementedException();
    }

}
";
}

public record ClassData
{
    public string Namespace { get; init; } = string.Empty;
    public string NamespacePath => Namespace.Replace('.', '/'); // Computed property for path
    public string ClassName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IncludeConstructor { get; init; }
    public List<PropertyData> Properties { get; init; } = new();
    public List<MethodData> Methods { get; init; } = new();
}

public record PropertyData
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? DefaultValue { get; init; }
}

public record MethodData
{
    public string Name { get; init; } = string.Empty;
    public string ReturnType { get; init; } = string.Empty;
    public string Parameters { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
