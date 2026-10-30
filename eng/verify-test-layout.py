# Vendored from the workspace shared tooling. Do not patch here: re-vendor the upstream file.
# Source: D:/lowlandtech/core/eng/verify-test-layout.py
# Upstream commit: d870ad9 (test(CORE0004-US01-SC01): add the Core owned scenario layout gate #9)
# Reason: CI has no access to the workspace checkout, so a pinned copy is the only way the
# behavior-test layout gate can actually run there instead of warning and passing.
"""Check that the Core scenarios follow the shared agentic-gherkin layout.

The Core lane owns its own layout gate. No other lane publishes a checker for
this repository, and a checker of another lane is not Core proof, so the rules
of the shared grammar are enforced here against the Core sources.

The gate reads the scenario files of the repository and reports every violation
that it finds. A scenario file is a file that declares a user story or a Given,
When or Then step; the other test files of the repository are legacy test
classes, and the gate inventories them without holding them to the scenario
rules.

The rules:

  invalid-scenario-path    a scenario file does not live at
                           ``src/test/<SCOPE>.<Area>/US##.<Story>/SC##_<Name>.cs``
  missing-given            a scenario declares no Given step
  multiple-given           a scenario declares more than one Given step
  missing-when             a scenario declares no When step
  multiple-when            a scenario declares more than one When step
  invalid-then-step        a Then step matches none of the forms of the grammar
  missing-uac-trait        a Fact carries no UAC trait
  duplicate-uac-trait      two Facts of a scenario carry the same UAC trait
  multiple-assertions      a Fact body holds more than one assertion
  non-expression-fact      a Fact is not a single-line expression-bodied member
  empty-selection          a scenario declares no Fact at all
  unfinished-skip          a Fact of a scenario is still skipped
  invalid-class-name       a scenario class is not named ``SC##_ScenarioName``
  multiple-types           a scenario file declares more than one named type
  scenario-too-long        a scenario file is longer than 100 lines

Exit codes:
  0  every scenario of the tree follows the layout
  1  the tree holds layout violations
  2  the tree could not be read
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

#: The directories that never hold sources of the repository.
PRUNED_DIRECTORIES = frozenset({"bin", "obj", "node_modules"})

#: The longest scenario file that the layout accepts.
MAX_SCENARIO_LINES = 100

#: The shape of a scenario path below the test root.
SCENARIO_PATH_PATTERN = re.compile(
    r"^(?P<scope>[A-Za-z0-9]+)\.(?P<area>[A-Za-z0-9]+)/"
    r"US(?P<story>\d+)\.(?P<storyName>[A-Za-z0-9]+)/"
    r"SC(?P<scenario>\d+)_[A-Za-z0-9_]+\.cs$"
)

_CLASS_PATTERN = re.compile(
    r"^\s*(?:\[[^\]]*\]\s*)*"
    r"(?:public|internal|private|protected)?\s*"
    r"(?:sealed\s+|abstract\s+|static\s+|partial\s+)*"
    r"(?P<kind>class|struct|interface|record)\s+(?P<name>[A-Za-z0-9_]+)"
)

_SCENARIO_CLASS_PATTERN = re.compile(r"^SC(?P<number>\d+)_[A-Za-z0-9_]+$")

_GIVEN_PATTERN = re.compile(r"\[\s*Given\s*\(")
_WHEN_PATTERN = re.compile(r"\[\s*When\s*\(")
_THEN_PATTERN = re.compile(r"\[\s*Then\s*\(\s*\"(?P<text>[^\"]*)\"")
_FACT_DECLARATION_PATTERN = re.compile(r"\bFact\b(?=\s*[\(\],])")
_UAC_PATTERN = re.compile(r"\[\s*Trait\s*\(\s*\"UAC\"\s*,\s*\"(?P<number>[^\"]*)\"")
_SKIP_PATTERN = re.compile(r"\bSkip\s*=")
_ASSERTION_PATTERN = re.compile(r"\.Should[A-Za-z]*\s*\(")

#: The forms that a Then step of the grammar may take.
_THEN_FORMS = (
    re.compile(r"^.+ should be \S+$"),
    re.compile(r"^.+ should not be null$"),
    re.compile(r"^.+ should be null$"),
    re.compile(r"^.+ should be true$"),
    re.compile(r"^.+ should be false$"),
    re.compile(r"^.+ event should be dispatched$"),
    re.compile(r"^.+ event should not be dispatched$"),
    re.compile(r"^.+ should contain .+$"),
    re.compile(r"^.+ should not contain .+$"),
    re.compile(r"^.+ should have count \d+$"),
    re.compile(r"^.+ should fail with \S+$"),
    re.compile(r"^.+ should be rejected$"),
    re.compile(r"^.+ should be accepted$"),
    re.compile(r"^.+ should be unchanged$"),
)


def _is_scenario(text: str) -> bool:
    """Whether a file declares a user story or a Given, When or Then step."""
    return bool(
        re.search(r"\[\s*UserStory\s*\(", text)
        or _GIVEN_PATTERN.search(text)
        or _WHEN_PATTERN.search(text)
        or _THEN_PATTERN.search(text)
    )


def _facts_of(text: str) -> list[dict[str, object]]:
    """The Facts of a scenario file, with the shape that the gate checks."""
    facts: list[dict[str, object]] = []
    lines = text.splitlines()
    for index, line in enumerate(lines):
        if not _FACT_DECLARATION_PATTERN.search(line):
            continue
        attributes = line[line.find("["):] if "[" in line else line
        uac = _UAC_PATTERN.search(line)
        body_lines: list[str] = []
        cursor = index + 1
        while cursor < len(lines):
            candidate = lines[cursor]
            if not candidate.strip():
                cursor += 1
                continue
            body_lines.append(candidate)
            if "=>" in candidate and candidate.rstrip().endswith(";"):
                break
            if "{" in candidate:
                depth = candidate.count("{") - candidate.count("}")
                while depth > 0 and cursor + 1 < len(lines):
                    cursor += 1
                    body_lines.append(lines[cursor])
                    depth += lines[cursor].count("{") - lines[cursor].count("}")
                break
            cursor += 1
        body = "\n".join(body_lines)
        facts.append({
            "line": index + 1,
            "uac": uac.group("number") if uac else None,
            "skipped": bool(_SKIP_PATTERN.search(attributes)),
            "assertions": len(_ASSERTION_PATTERN.findall(body)),
            "expressionBodied": "=>" in body and body.rstrip().endswith(";"),
            "lines": len([line for line in body_lines if line.strip()]),
        })
    return facts


def _then_steps(text: str) -> list[str]:
    """The Then steps of a scenario file."""
    return [match.group("text").strip() for match in _THEN_PATTERN.finditer(text)]


def scan(root: Path) -> dict[str, object]:
    """Scan the test sources of a root and report the scenarios and the violations."""
    scenarios: list[dict[str, object]] = []
    legacy: list[dict[str, object]] = []
    violations: list[dict[str, str]] = []

    for path in sorted(root.rglob("*.cs")):
        if any(part in PRUNED_DIRECTORIES for part in path.parts):
            continue
        try:
            text = path.read_text(encoding="utf-8", errors="replace")
        except OSError:
            continue
        relative = path.relative_to(root).as_posix()
        if not _is_scenario(text):
            facts = _facts_of(text)
            if facts:
                legacy.append({
                    "file": relative,
                    "facts": len(facts),
                    "skipped": len([fact for fact in facts if fact["skipped"]]),
                })
            continue

        lines = text.splitlines()
        given = len(_GIVEN_PATTERN.findall(text))
        when = len(_WHEN_PATTERN.findall(text))
        facts = _facts_of(text)
        types = [_CLASS_PATTERN.search(line) for line in lines]
        types = [match.group("name") for match in types if match]
        scenario_types = [name for name in types if _SCENARIO_CLASS_PATTERN.match(name)]

        steps = _then_steps(text)
        for step in steps:
            if not any(form.match(step) for form in _THEN_FORMS):
                violations.append({
                    "code": "invalid-then-step",
                    "file": relative,
                    "detail": f"The step '{step}' matches no form of the grammar.",
                })

        if not re.match(SCENARIO_PATH_PATTERN, relative):
            violations.append({
                "code": "invalid-scenario-path",
                "file": relative,
                "detail": "A scenario file does not live at "
                          "src/test/<SCOPE>.<Area>/US##.<Story>/SC##_<Name>.cs.",
            })
        if given == 0:
            violations.append({
                "code": "missing-given", "file": relative,
                "detail": "The scenario declares no Given step.",
            })
        if given > 1:
            violations.append({
                "code": "multiple-given", "file": relative,
                "detail": f"The scenario declares {given} Given steps.",
            })
        if when == 0:
            violations.append({
                "code": "missing-when", "file": relative,
                "detail": "The scenario declares no When step.",
            })
        if when > 1:
            violations.append({
                "code": "multiple-when", "file": relative,
                "detail": f"The scenario declares {when} When steps.",
            })
        if not facts:
            violations.append({
                "code": "empty-selection", "file": relative,
                "detail": "The scenario declares no Fact.",
            })

        seen_uac: dict[str, int] = {}
        for fact in facts:
            location = f"{relative}:{fact['line']}"
            uac = fact["uac"]
            if uac is None:
                violations.append({
                    "code": "missing-uac-trait", "file": location,
                    "detail": "The Fact carries no UAC trait.",
                })
            else:
                if uac in seen_uac:
                    violations.append({
                        "code": "duplicate-uac-trait", "file": location,
                        "detail": f"The UAC trait {uac} is already used at line {seen_uac[uac]}.",
                    })
                seen_uac[uac] = int(fact["line"])
            if fact["skipped"]:
                violations.append({
                    "code": "unfinished-skip", "file": location,
                    "detail": "The Fact of the scenario is still skipped.",
                })
            if int(fact["assertions"]) != 1:
                violations.append({
                    "code": "multiple-assertions", "file": location,
                    "detail": f"The Fact holds {fact['assertions']} assertions instead of one.",
                })
            if not fact["expressionBodied"]:
                violations.append({
                    "code": "non-expression-fact", "file": location,
                    "detail": "The Fact is not a single-line expression-bodied member.",
                })

        if not scenario_types:
            violations.append({
                "code": "invalid-class-name", "file": relative,
                "detail": "The scenario declares no class named SC##_ScenarioName.",
            })
        if len(types) > 1:
            violations.append({
                "code": "multiple-types", "file": relative,
                "detail": f"The scenario file declares {len(types)} named types.",
            })
        if len(lines) > MAX_SCENARIO_LINES:
            violations.append({
                "code": "scenario-too-long", "file": relative,
                "detail": f"The scenario file holds {len(lines)} lines.",
            })

        scenarios.append({
            "file": relative,
            "given": given,
            "when": when,
            "facts": facts,
            "thenSteps": steps,
            "lines": len(lines),
            "types": types,
        })

    return {
        "root": str(root),
        "scenarios": scenarios,
        "legacyTests": legacy,
        "violations": violations,
        "counts": {
            "scenarios": len(scenarios),
            "legacyTestFiles": len(legacy),
            "legacyFacts": sum(int(entry["facts"]) for entry in legacy),
            "scenarioFacts": sum(len(entry["facts"]) for entry in scenarios),
            "violations": len(violations),
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", default=None, help="the test root to scan")
    parser.add_argument("--json", action="store_true", help="print the evidence as JSON")
    arguments = parser.parse_args()

    root = Path(arguments.root).resolve() if arguments.root else Path(__file__).resolve().parents[1] / "src" / "test"
    if not root.is_dir():
        print(f"the test root {root} does not exist", file=sys.stderr)
        return 2

    evidence = scan(root)

    if arguments.json:
        print(json.dumps(evidence, indent=2))
    else:
        print("Core scenario layout")
        print(f"  root: {root}")
        counts = evidence["counts"]
        print(f"  scenarios: {counts['scenarios']}, legacy test files: {counts['legacyTestFiles']}, "
              f"legacy Facts: {counts['legacyFacts']}")
        for scenario in evidence["scenarios"]:
            print(f"  scenario: {scenario['file']} "
                  f"(given {scenario['given']}, when {scenario['when']}, facts {len(scenario['facts'])})")
        for violation in evidence["violations"]:
            print(f"  VIOLATION [{violation['code']}] {violation['file']}: {violation['detail']}")

    return 1 if evidence["violations"] else 0


if __name__ == "__main__":
    sys.exit(main())
