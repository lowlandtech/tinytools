# Workflow CLI

`tinytools` turns a YAML workflow into a workspace. Template steps are rendered with the existing TinyTemplateEngine; output files are written only when their content changes. Templates can be kept in a separate reusable pack with `--templates`, and `forEach` steps can generate one file per model item.

```bash
dotnet run --project src/cli -- generate workflow.yml --model model.json --root ./workspace --templates ./templates
```

Example:

```yaml
name: workspace
variables:
  Product: TinyTools
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

Template and command working-directory paths are confined to `--root`. Command steps currently allow only `dotnet`, `git`, and `gh`; arguments are passed without a shell. This makes workflows predictable for agents and avoids accidental shell expansion.

The shared contract is in `LowlandTech.TinyTools.Workflows`, so MAUI and Blazor WASM hosts can load and display the same workflow definitions.
