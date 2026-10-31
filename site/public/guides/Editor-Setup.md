# Editor Setup: Templates in Your Editor

TinyTools ships a language server, so any editor that speaks LSP gets template diagnostics,
completion and a revision-gated preview. There is no editor-specific plugin to install: the
server is a plain stdio process that the editor starts.

## Run the server

```bash
dotnet build src/lsp/LowlandTech.TinyTools.LanguageServer.csproj -c Release
dotnet src/lsp/bin/Release/net10.0/tinytools-lsp.dll
```

The process speaks LSP 3.17 over stdin and stdout and nothing else, so any client can own it.

## What it advertises

```json
{
  "capabilities": {
    "textDocumentSync": 1,
    "completionProvider": { "triggerCharacters": ["$", "."] },
    "executeCommandProvider": { "commands": ["tinytools.preview"] }
  }
}
```

`initializationOptions` tells the server which model the template edits against:

| Option | Meaning |
|---|---|
| `modelMembers` | explicit member names, for example `["Name", "Items", "Count"]` |
| `modelType` | an assembly-qualified type name the server resolves and reflects over |

## What you get

- **Diagnostics** on open and on change, using the engine's own codes (`TS001` for an
  interpolation that is never closed, and so on) with line and column.
- **Completion** without a trigger character lists the pipe helpers (`upper`, `where`, `format`,
  and 37 more as of this writing). Type `.` to get the model members you declared.
- **Preview** through `workspace/executeCommand` with command `tinytools.preview`, arguments
  `[uri, revision]`. It renders the open document against an empty context and returns
  `{ text, revision, stale }`; `stale` is true when you asked for an older revision than the
  server has seen.

## Verify your client

```bash
python eng/lsp-session-check.py
```

That script speaks the protocol to the built server exactly as a client would and asserts the
capabilities, the diagnostics, both completion lists, the preview and a clean shutdown. It is the
same check that proves a change to the server did not break editors.

## VS Code

Any generic LSP client works; the settings below are the ones that matter.

```json
{
  "tinytools.serverPath": "src/lsp/bin/Release/net10.0/tinytools-lsp.dll",
  "tinytools.initializationOptions": { "modelMembers": ["Name", "Items"] }
}
```

Point your LSP client at `dotnet <serverPath>` with the language id `tinytools` for `*.tpl` and
`*.tmpl` files.

## Visual Studio

Visual Studio can host an LSP client through its language server protocol support, with the same
`dotnet <serverPath>` command. A packaged VSIX is not published yet: it needs a Visual Studio SDK
that matches the Visual Studio release it would target, and building one against an older SDK
produces an extension that will not load. Until that exists, the server above is the supported
path in both editors and the behaviour is identical.
