# TinyTemplateEngine Product Guide

TinyTemplateEngine (LowlandTech.TinyTools) is a lightweight .NET template engine built for data composition, not view rendering. It keeps templates predictable, avoids runtime compilation, and focuses on merging structured data into text.

## What This Library Solves

- Data-first templating without HTML bias.
- Deterministic outputs for JSON, Markdown, config files, prompts, and emails.
- Minimal dependencies with a small, explicit surface area.

## Core Concepts

- `ExecutionContext` stores values and services accessible through `Context`.
- `TinyTemplateEngine` renders templates with `@if`, `@foreach`, and helper pipes.
- String extension methods provide quick interpolation for simple cases.

## Feature Map

- Variable interpolation with `${Context.Name}` and `${Context.Model.Name}`.
- Control flow (`@if`, `else if`, `else`, `@foreach`) with comparison operators.
- Null coalescing (`${Context.Name ?? "Guest"}`) for defaults.
- Pipe helpers (`${Context.Name | upper}`) for transformations.
- Template services for custom transformations and external logic.
- Comment syntax `@* ... *@` for readable templates without output noise.

## Guides

- `docs/product/interpolation.md`
- `docs/product/control-flow.md`
- `docs/product/helpers.md`
- `docs/product/template-services.md`

## Quick Start

```csharp
var engine = new TinyTemplateEngine();
var context = new ExecutionContext();
context.Set("Name", "John");

var template = "Hello ${Context.Name}";
var result = engine.Render(template, context);
// Hello John
```

## Design Principles

- Templates describe placement and shape, not application logic.
- Data is prepared outside the template and passed in through context.
- Helper pipes and services keep transformations explicit and testable.
