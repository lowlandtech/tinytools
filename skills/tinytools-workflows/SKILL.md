---
name: tinytools-workflows
description: Generate and update workspaces with the TinyTools YAML workflow CLI and .tt templates. Use when an agent needs repeatable, idempotent file generation, workspace bootstrapping, or a workflow combining templates with dotnet, git, or gh steps.
---

# TinyTools Workflows

Use the repository CLI to generate files from a YAML workflow. Treat the workflow as the reproducible source of truth and keep generated files under the requested workspace root.

## Execute a workflow

1. Inspect the repository and identify the workflow file, template directory, model file, and output root.
2. Run:

   ```bash
   dotnet run --project src/cli -- generate <workflow.yml> --model <model.json> --root <workspace> --templates <template-root>
   ```

   Omit `--model` when the workflow does not need one. `--templates` is optional and defaults to the workflow file's directory.
3. Review the changed/unchanged step summary and inspect generated files.
4. Re-run the same command to verify idempotency. Template outputs should become `unchanged` when inputs have not changed.

## Author a workflow

Use `variables` for fixed workflow values and `model` for workspace-specific input. Template steps require `template` and `output` paths relative to the root:

```yaml
name: workspace
variables:
  Product: Example
steps:
  - id: readme
    type: template
    template: templates/README.md.tt
    output: README.md
  - id: restore
    type: command
    command: dotnet
    args: [restore]
```

Inside `.tt` files, use TinyTools expressions such as `${Context.Product}` and `${Context.Model.Name}`. Keep templates deterministic and free of timestamps or random values unless explicitly requested.

## Command safety and idempotency

- Only `dotnet`, `git`, and `gh` command names are supported.
- Pass command arguments through `args`; do not construct shell command strings.
- Keep template and command working-directory paths inside `--root`.
- Prefer commands that are naturally safe to repeat, such as `dotnet restore` or read-only inspection.
- Do not add destructive commands to a workflow without explicit user approval.
- If a command is not repeat-safe, split it out for confirmation rather than hiding it in generation.

Read [workflow-schema.md](references/workflow-schema.md) when adding or reviewing workflow fields.

## GraphStrip bootstrap

Use the bundled GraphStrip pack when bootstrapping the migration-reset repository:

```bash
dotnet run --project src/cli -- generate skills/tinytools-workflows/assets/graphstrip/workflow.yml --model skills/tinytools-workflows/assets/graphstrip/model.json --root D:/graphstrip --templates skills/tinytools-workflows/assets/graphstrip
```

The pack creates the `src/core`, `src/domain`, `src/plugins`, `src/shell`, `src/api`, `src/client`, `src/app`, and `src/test` project shells, shared plugin contracts and registry, plugin registration, host entry points, one generated plugin/test/workitem per model entry, and a solution file. Treat generated plugins and tests as refinement starting points; replace their TODOs before implementation and register concrete behavior through the typed intent boundary.
