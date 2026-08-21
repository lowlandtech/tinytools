# Workflow schema

`WorkflowDefinition` contains:

- `name`: optional display name
- `variables`: string/object values copied into the template context
- `steps`: ordered operations

Each `WorkflowStep` supports:

- `id`: optional stable identifier
- `type`: `template` or another type handled by the CLI
- `template`: source `.tt` path for template steps
- `output`: destination path for template steps
- `command`: executable name for command steps
- `args`: argument list passed directly to the process
- `workingDirectory`: optional directory relative to the workflow root
- `when`: currently supports `always` and `if-missing` for template outputs
- `forEach`: optional context collection expression; repeats the step once per item
- `item`: context variable name for the current `forEach` item (defaults to `item`)

The CLI converts JSON models into ordinary dictionaries/lists before passing them to the template engine, so nested `${Context.Model.Property}` expressions resolve consistently.

Template paths are resolved from `--templates`, while output and command paths are resolved from `--root`. This lets a reusable skill ship templates separately from the target repository.
