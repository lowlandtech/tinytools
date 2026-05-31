# TinyTools Feature Ideas

This document collects small, focused feature ideas that fit TinyTools' core philosophy:

> Keep templates tiny, deterministic, dependency-light, and useful for data composition.

These ideas are not commitments. They are candidates to evaluate against the roadmap, implementation cost, and the project's goal of staying intentionally small.

---

## Recommended First Feature

### Template Diagnostics and Strict Mode

The strongest next feature candidate is a diagnostics layer with optional strict rendering.

Today, TinyTools is already useful for rendering deterministic text. The next quality-of-life improvement would be helping users find template mistakes before output is generated or shipped.

Possible API:

```csharp
var diagnostics = engine.Analyze(template);

foreach (var diagnostic in diagnostics)
{
    Console.WriteLine($"{diagnostic.Line}:{diagnostic.Column} {diagnostic.Message}");
}
```

Strict rendering could build on the same analysis model:

```csharp
var engine = new TinyTemplateEngine(new TinyTemplateOptions
{
    StrictVariables = true,
    StrictHelpers = true
});
```

Suggested diagnostics:

- Missing or unresolved variables
- Unknown pipe helpers
- Unmatched `@if`, `@foreach`, `@else`, or closing braces
- Invalid null-coalescing or ternary expressions
- Line and column information for template errors
- Suggestions for common mistakes, such as misspelled helper names

Why it fits:

- Improves trust without making templates more powerful or complex
- Supports existing validation work
- Helps CLI, editor tooling, and future source-generator scenarios
- Keeps error handling deterministic and easy to test

---

## Other Strong Candidates

### Render From JSON

TinyTools is often used for data composition, and JSON is a natural input format for templates. A first-class JSON render path would make examples, tests, CLIs, and automation easier.

Possible API:

```csharp
var output = engine.RenderJson(template, json);
```

Possible overloads:

```csharp
var output = engine.RenderJson(template, jsonString);
var output = engine.RenderJson(template, jsonDocument);
var output = engine.RenderJsonFile(template, "data.json");
```

Why it fits:

- Aligns with generating JSON, YAML, Markdown, config files, prompts, and emails
- Makes CLI rendering easier to implement later
- Gives users a simple bridge from external data into `ToolContext`

### Escaping Helpers

TinyTools is format-agnostic, so it should help users safely project values into common text formats without becoming an HTML view engine.

Possible helpers:

```text
${Context.Name | json}
${Context.Description | yaml}
${Context.Value | csv}
${Context.Text | markdown}
```

Why it fits:

- Supports non-HTML output formats
- Keeps escaping explicit in the template
- Reduces accidental invalid JSON, CSV, YAML, or Markdown output

### Template Includes

Includes would allow reusable fragments while keeping templates simple.

Possible syntax:

```text
@include("partials/header.tmpl")

Content here

@include("partials/footer.tmpl")
```

Recommended design:

```csharp
public interface ITemplateLoader
{
    string Load(string path);
}
```

Why it fits:

- Enables reusable headers, footers, prompts, and generated-file fragments
- Keeps file-system behavior outside the engine
- Allows in-memory, embedded-resource, and file-based loaders

### Tiny CLI

A small command-line tool would make TinyTools useful outside application code.

Possible commands:

```bash
tinytools render --template email.tmpl --data data.json --out email.txt
tinytools validate --template email.tmpl --data data.json
tinytools watch --template "*.tmpl" --data data.json --out output
```

Why it fits:

- Makes the library easier to try
- Supports automation and code-generation workflows
- Provides a practical surface for diagnostics and JSON rendering

---

## Lower Priority Ideas

### File-Based Template Registry

Allow a registry to discover `.tmpl` files from a folder and render them by name.

This is useful, but it should probably come after includes and diagnostics so file templates have a clearer error model.

### Editor Tooling

Syntax highlighting, diagnostics, and preview tooling would be valuable. This should likely build on the diagnostics API rather than being implemented first.

### Source Generator Templates

Build-time template generation could be powerful, but it has a larger maintenance surface. It should probably wait until the parser, diagnostics, and strict behavior are stable.

---

## Guardrails

These feature ideas should preserve the current TinyTools boundaries:

- No HTML-first view rendering
- No runtime code execution
- No JavaScript engine
- No heavy dependencies
- No broad template DSL expansion
- No hidden global file-system behavior

TinyTools should stay boring in the best way: predictable, inspectable, and easy to reason about.
