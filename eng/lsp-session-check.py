#!/usr/bin/env python3
"""Drive the template language server the way an editor does.

The server exists so that any editor can offer template completion, diagnostics and preview
without a Visual Studio extension. That claim is only worth as much as a session that actually
runs, so this script spawns the built server over stdio, speaks LSP to it and asserts what an
editor would see.

Usage, from the repository root:
    python eng/lsp-session-check.py
    python eng/lsp-session-check.py --json
"""

import argparse
import json
import os
import queue
import subprocess
import sys
import threading
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / "src/lsp/LowlandTech.TinyTools.LanguageServer.csproj"
ASSEMBLY = "tinytools-lsp.dll"


class Session:
    """Minimal LSP client: Content-Length framing over the server's stdio."""

    def __init__(self, process):
        self.process = process
        self.messages = queue.Queue()
        self.reader = threading.Thread(target=self._read, daemon=True)
        self.reader.start()

    def _read(self):
        stream = self.process.stdout
        while True:
            headers = {}
            while True:
                line = stream.readline()
                if not line:
                    self.messages.put(None)
                    return
                text = line.decode("ascii", "replace").strip()
                if not text:
                    break
                name, _, value = text.partition(":")
                headers[name.strip().lower()] = value.strip()
            length = int(headers.get("content-length", "0"))
            if length == 0:
                continue
            self.messages.put(json.loads(stream.read(length).decode("utf-8")))

    def send(self, payload):
        body = json.dumps(payload).encode("utf-8")
        self.process.stdin.write(b"Content-Length: %d\r\n\r\n" % len(body) + body)
        self.process.stdin.flush()

    def request(self, identifier, method, params=None):
        self.send(dict(jsonrpc="2.0", id=identifier, method=method, params=params))
        return self.await_id(identifier)

    def notify(self, method, params=None):
        self.send(dict(jsonrpc="2.0", method=method, params=params))

    def next_message(self, timeout=20.0):
        try:
            message = self.messages.get(timeout=timeout)
        except queue.Empty:
            raise TimeoutError("no message from the language server within %.0fs" % timeout)
        if message is None:
            raise RuntimeError("the language server closed its output")
        return message

    def await_id(self, identifier, timeout=20.0):
        deadline = time.time() + timeout
        while time.time() < deadline:
            message = self.next_message(timeout=max(0.1, deadline - time.time()))
            if message.get("id") == identifier:
                return message
        raise TimeoutError("no reply to request %s" % identifier)

    def await_notification(self, method, timeout=20.0):
        deadline = time.time() + timeout
        while time.time() < deadline:
            message = self.next_message(timeout=max(0.1, deadline - time.time()))
            if message.get("method") == method:
                return message
        raise TimeoutError("no %s notification" % method)


def build(quiet):
    result = subprocess.run(["dotnet", "build", str(PROJECT), "--configuration", "Release"],
                            cwd=ROOT, capture_output=True, text=True, encoding="utf-8")
    if result.returncode:
        raise RuntimeError("the language server did not build:\n" + (result.stdout or "")[-2000:])
    if not quiet:
        print("built " + PROJECT.name)


def run_checks(quiet):
    build(quiet)
    assembly = next(PROJECT.parent.glob("bin/Release/*/" + ASSEMBLY), None)
    if assembly is None:
        raise RuntimeError("no built " + ASSEMBLY)

    checks = []
    stage = ["start"]

    def check(name, passed, observed):
        checks.append(dict(name=name, passed=bool(passed), observed=observed))

    process = subprocess.Popen(["dotnet", str(assembly)], cwd=ROOT,
                               stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    try:
        stage[0] = "initialize"
        session = Session(process)
        initialized = session.request(1, "initialize", {
            "processId": os.getpid(),
            "rootUri": ROOT.as_uri(),
            "capabilities": {},
            "initializationOptions": {"modelMembers": ["Name", "Items", "Count"]},
        })
        capabilities = (initialized.get("result") or {}).get("capabilities") or {}
        check("initialize returns capabilities", bool(capabilities), sorted(capabilities))
        check("completion is advertised with its triggers",
              (capabilities.get("completionProvider") or {}).get("triggerCharacters") == ["$", "."],
              capabilities.get("completionProvider"))
        check("the preview command is advertised",
              (capabilities.get("executeCommandProvider") or {}).get("commands") == ["tinytools.preview"],
              capabilities.get("executeCommandProvider"))
        session.notify("initialized", {})

        stage[0] = "didOpen"
        uri = (ROOT / "templates/session.tpl").as_uri()
        session.notify("textDocument/didOpen", {"textDocument": {
            "uri": uri, "languageId": "tinytools", "version": 1,
            "text": "Hello ${Context.Model.Name}!",
        }})
        session.await_notification("textDocument/publishDiagnostics")

        stage[0] = "didChange"
        session.notify("textDocument/didChange", {
            "textDocument": {"uri": uri, "version": 2},
            "contentChanges": [{"text": "Hello ${Context.Model.Name"}],
        })
        published = session.await_notification("textDocument/publishDiagnostics")
        diagnostics = (published.get("params") or {}).get("diagnostics") or []
        check("a malformed template is reported", len(diagnostics) > 0, len(diagnostics))
        check("the diagnostic carries the engine code",
              bool(diagnostics) and str(diagnostics[0].get("code", "")).startswith("TS"),
              diagnostics[0].get("code") if diagnostics else None)

        stage[0] = "completion"
        helper_completion = session.request(2, "textDocument/completion", {
            "textDocument": {"uri": uri},
            "position": {"line": 0, "character": 6},
        })
        helper_result = helper_completion.get("result") or []
        helpers = helper_result.get("items") if isinstance(helper_result, dict) else helper_result
        helper_labels = sorted(item.get("label") for item in helpers if isinstance(item, dict))
        check("completion offers the pipe helpers", {"upper", "where", "format"} <= set(helper_labels),
              len(helper_labels))

        member_completion = session.request(6, "textDocument/completion", {
            "textDocument": {"uri": uri},
            "position": {"line": 0, "character": 6},
            "context": {"triggerCharacter": "."},
        })
        member_result = member_completion.get("result") or []
        members = member_result.get("items") if isinstance(member_result, dict) else member_result
        member_labels = sorted(item.get("label") for item in members if isinstance(item, dict))
        check("the dot trigger offers the declared model members",
              {"Name", "Items", "Count"} <= set(member_labels), member_labels)

        stage[0] = "didChange-valid"
        session.notify("textDocument/didChange", {
            "textDocument": {"uri": uri, "version": 3},
            "contentChanges": [{"text": "Hello ${Context.Model.Name}!"}],
        })
        session.await_notification("textDocument/publishDiagnostics")

        stage[0] = "preview"
        preview = session.request(3, "workspace/executeCommand", {
            "command": "tinytools.preview",
            "arguments": [uri, 3],
        })
        rendered = preview.get("result") or {}
        # The preview renders the open document against an empty context, so the expression resolves
        # to nothing; the point is that the engine ran inside the server and honoured the revision.
        check("the preview command renders the open document",
              rendered.get("text") == "Hello !", rendered.get("text"))
        check("the preview reports its revision", rendered.get("revision") == 3, rendered.get("revision"))

        stage[0] = "malformed command"
        rejected = session.request(5, "workspace/executeCommand", {
            "command": "tinytools.preview",
            "arguments": [{"unexpected": "shape"}],
        })
        check("a malformed command is answered, not fatal",
              "result" in rejected and rejected.get("error") is None,
              (rejected.get("result") or {}).get("stale"))

        stage[0] = "shutdown"
        shutdown = session.request(4, "shutdown", None)
        check("shutdown is acknowledged", "result" in shutdown and shutdown.get("error") is None,
              shutdown.get("result"))
    except Exception as error:
        process.kill()
        detail = (process.stderr.read() or b"").decode("utf-8", "replace")[-1500:]
        raise RuntimeError("failed during %s: %s\n%s" % (stage[0], error, detail))
    finally:
        if process.stdin and not process.stdin.closed:
            process.stdin.close()
        try:
            process.wait(timeout=15)
        except subprocess.TimeoutExpired:
            process.kill()
    return checks


def main(argv=None):
    parser = argparse.ArgumentParser(description="Drive the template language server over LSP.")
    parser.add_argument("--json", action="store_true", help="print the result as JSON")
    parser.add_argument("--quiet", action="store_true")
    arguments = parser.parse_args(argv)
    try:
        checks = run_checks(arguments.quiet)
    except (RuntimeError, TimeoutError, OSError, subprocess.SubprocessError) as error:
        payload = dict(ok=False, error=str(error), checks=[])
    else:
        failed = [c for c in checks if not c["passed"]]
        payload = dict(ok=not failed, checks=checks, failed=len(failed))
        for entry in checks:
            if not arguments.quiet:
                print("%s %s: %s" % ("PASS" if entry["passed"] else "FAIL", entry["name"],
                                    json.dumps(entry["observed"])))
    if arguments.json or not payload["ok"]:
        print(json.dumps(payload))
    return 0 if payload["ok"] else 1


if __name__ == "__main__":
    sys.exit(main())
