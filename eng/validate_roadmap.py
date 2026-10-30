# Vendored from the workspace shared tooling. Do not patch here: re-vendor the upstream file.
# Source: D:/lowlandtech/eng/harness/validate_roadmap.py
# Upstream commit: 58beaf6 (chore: preserve shared agent operations and database recovery checkpoints)
# Reason: the roadmap contract is quoted in every closure, so CI must be able to enforce it.
"""Validate the sole, tables-only roadmap and source-owned workitem contracts."""
import argparse
import json
from pathlib import Path
import re
import sys


MILESTONES = ['Milestone', 'Title', 'Gate']
WORKITEMS = ['Work', 'Scope', 'Title', 'Priority', 'Lane', 'Depends on', 'Status', 'Refined', 'Issue']
WORKITEMS_LEGACY = WORKITEMS[:-1]
FORBIDDEN = re.compile(r'(?:^|[-_. ])(?:roadmaps?|milestones?)(?:$|[-_. ])|^(?:m|s)\d+(?:$|[-_. ])', re.I)
IDENTITY = re.compile(r'[A-Z]{4}\d{4}-US\d{2}-SC\d{2,3}')


def validate(root):
    root = Path(root).resolve()
    docs = root / 'docs'
    board = docs / 'workitems/README.md'
    diagnostics = []

    def fail(code, path, line, message):
        diagnostics.append(dict(code=code, path=str(path.relative_to(root)).replace('\\', '/'),
                                line=line, message=message))

    if not board.is_file():
        fail('RMP001', board, 0, 'The sole roadmap docs/workitems/README.md is missing.')
        return dict(ok=False, milestones=0, workitems=0, diagnostics=diagnostics)
    for path in docs.rglob('*'):
        if path == board:
            continue
        source_unit = (path.is_file() and path.parent.parent == board.parent
                       and path.parent.name in ['todo', 'doing', 'done']
                       and re.fullmatch(r'[A-Z]{4}\d{4}\.US\d{2}\.SC\d{2,3}\..+\.md', path.name))
        if path.is_file() and path.parent == board.parent:
            fail('RMP022', path, 0, 'The workitems root may contain only README.md; place source units in todo/doing/done and remove superseded aliases.')
        if path.is_file() and path.suffix.lower() == '.md' and not source_unit:
            fail('RMP021', path, 0, 'Narrative specifications belong in workitems; reusable agent guidance belongs only in shared skills. Repository docs guides/indexes are forbidden.')
        if (not source_unit and FORBIDDEN.search(path.name)) or (path.is_dir() and path.name.lower() in ['sprint', 'sprints']):
            fail('RMP002', path, 0, 'Separate roadmap/milestone files and directories are forbidden; use the sole board and owning workitems.')
        if path.is_file() and path.suffix.lower() == '.md' and 'workitems' not in path.relative_to(docs).parts:
            content = path.read_text(encoding='utf-8-sig')
            if (re.search(r'^\|\s*(?:Work\s*\|\s*Scope|Scope\s*\|\s*Work ?item|Milestone\s*\|)\s*', content, re.M)
                    or re.search(r'^##\s+M\d+\s*[-—]', content, re.M)
                    or re.search(r'^##\s+Ordered delivery (?:path|plan)\s*$', content, re.M | re.I)):
                fail('RMP003', path, 0, 'A second milestone/workitem planning board is forbidden.')
        if path.is_file() and path.suffix.lower() == '.json' and 'workitems' not in path.relative_to(docs).parts:
            try:
                data = json.loads(path.read_text(encoding='utf-8-sig'))
            except (ValueError, UnicodeError):
                continue
            if isinstance(data, list) and data and all(isinstance(row, dict) and
                    'scope' in row and 'workitem' in row and ('packet' in row or 'status' in row) for row in data):
                fail('RMP003', path, 0, 'A duplicate backlog planning index is forbidden; keep specifications in source workitems.')

    tables = []
    current = []
    for number, line in enumerate(board.read_text(encoding='utf-8-sig').splitlines(), 1):
        if not line.strip():
            if current:
                tables.append(current)
                current = []
            continue
        if not line.startswith('|') or not line.endswith('|') or '<!--' in line or '-->' in line:
            fail('RMP004', board, number, 'Only Markdown planning tables and blank separator lines are allowed; no headings, prose, comments or code.')
            continue
        current.append((number, [cell.strip() for cell in line[1:-1].split('|')]))
    if current:
        tables.append(current)

    milestones = {}
    workitems = []
    seen_scopes = set()
    linked_sources = set()
    for table in tables:
        header = table[0][1]
        if header not in [MILESTONES, WORKITEMS, WORKITEMS_LEGACY]:
            fail('RMP005', board, table[0][0], 'Expected a Milestone/Title/Gate or Work/Scope/Title/Priority/Lane/Depends on/Status/Refined[/Issue] planning table.')
            continue
        if len(table) < 3 or len(table[1][1]) != len(header) or not all(re.fullmatch(r':?-{3,}:?', cell) for cell in table[1][1]):
            fail('RMP006', board, table[0][0], 'Each table requires its separator and at least one planning row.')
            continue
        for number, cells in table[2:]:
            if len(cells) != len(header):
                fail('RMP007', board, number, 'Planning row column count differs from its header.')
                continue
            row = dict(zip(header, cells))
            if header == MILESTONES:
                milestone = row['Milestone']
                if not re.fullmatch(r'(?:M\d+[A-Z]?|PKG|CORB|OPT|TOOL|H)', milestone) or milestone in milestones or not row['Title'] or not row['Gate']:
                    fail('RMP008', board, number, 'Milestone IDs must be unique M<number> values with a title and gate.')
                else:
                    milestones[milestone] = row
                continue
            work, scope = row['Work'], row['Scope'].strip('`')
            if not re.fullmatch(r'(?:(?:M\d+[A-Z]?|PKG|CORB|OPT|TOOL)-\d{3}|H-US\d{2}-SC\d{2,3})', work) or any(item['Work'] == work for item in workitems):
                fail('RMP009', board, number, 'Work must contain one unique milestone workitem ID only.')
            if not IDENTITY.fullmatch(scope) or scope in seen_scopes:
                fail('RMP010', board, number, 'Scope must contain one unique SPECID-US##-SC## identity.')
            seen_scopes.add(scope)
            if row['Refined'] not in ['true', 'false']:
                fail('RMP011', board, number, 'Refined must be the literal true or false, independent of product status.')
            if header == WORKITEMS:
                issue = re.fullmatch(r'\[#([1-9]\d*)\]\(https://github\.com/lowlandtech/([^/]+)/issues/([1-9]\d*)\)', row['Issue'])
                if not issue or issue[1] != issue[3]:
                    fail('RMP024', board, number, 'Issue must link one matching #n on GitHub.')
            link = re.fullmatch(r'\[([^\[\]]+)\]\(([^)]+)\)', row['Title'])
            if not link:
                fail('RMP012', board, number, 'Title must contain exactly one link to its authoritative workitem.')
            else:
                target = (board.parent / link[2]).resolve()
                if not target.is_relative_to(board.parent.resolve()) or target.parent.name not in ['todo', 'doing', 'done'] or not target.is_file():
                    fail('RMP013', board, number, 'Workitem links must resolve inside a source-owned todo/doing/done lane.')
                else:
                    source = target.read_text(encoding='utf-8-sig')
                    fields = {}
                    for key, value in re.findall(r'^- \*\*([^:*]+):\*\*[ \t]*(.*)$', source, re.M):
                        fields.setdefault(key, value)
                    for key, expected in [('Scope delivered', scope), ('Milestone', work.split('-')[0]), ('Status', row['Status']), ('Priority', row['Priority']), ('Depends on', row['Depends on'])]:
                        if key == 'Milestone' and not fields.get(key):
                            continue  # The sole board owns planning; legacy specs need no duplicate field.
                        actual = fields.get(key, '').strip()
                        if key == 'Milestone':
                            actual = actual.split(' (', 1)[0]
                        if actual != expected:
                            fail('RMP014', board, number, f'{key} differs from the authoritative workitem.')
                    lane = {'todo': 'todo', 'blocked': 'todo', 'doing': 'doing', 'review': 'doing', 'done': 'done'}.get(row['Status'])
                    if target.parent.name != lane:
                        fail('RMP015', board, number, 'Source file lane does not match product status; refinement must not move it.')
                    if not row['Lane']:
                        fail('RMP016', board, number, 'Lane accountability is required; runner claims remain in source workitems.')
            if link and target.is_file():
                linked_sources.add(target)
                row['_source'] = source
            workitems.append(row)
    for lane_name in ['todo', 'doing']:
        lane_dir = board.parent / lane_name
        if not lane_dir.is_dir():
            continue
        for path in lane_dir.glob('*.md'):
            if re.fullmatch(r'[A-Z]{4}\d{4}\.US\d{2}\.SC\d{2,3}\..+\.md', path.name) and path.resolve() not in linked_sources:
                fail('RMP023', path, 0, 'Active source workitem is missing from the sole roadmap; add one linked Work row without changing its status or owner.')
    if not milestones or not workitems:
        fail('RMP017', board, 0, 'Both milestone definitions and linked workitem planning rows are required.')
    for row in workitems:
        if row['Work'].split('-')[0] not in milestones:
            fail('RMP018', board, 0, f"{row['Work']} has no milestone definition in this same board.")
        for dependency in row['Depends on'].split(','):
            dependency = dependency.strip()
            if (dependency.lower() not in ['', 'none']
                    and not any(dependency in [item['Work'], item['Scope'].strip('`')] for item in workitems)
                    and '<!-- external-prerequisite: ' + dependency + ' -->' not in row.get('_source', '')):
                fail('RMP019', board, 0, f"{row['Work']} has an unresolved local dependency {dependency}; external prerequisites belong in its source contract.")
    return dict(ok=not diagnostics, milestones=len(milestones), workitems=len(workitems), diagnostics=diagnostics)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repository-root', default=str(Path(__file__).resolve().parents[2] / 'harness'))
    parser.add_argument('--format', choices=['text', 'json'], default='text')
    args = parser.parse_args()
    try:
        if not Path(args.repository_root).is_dir():
            raise FileNotFoundError('Repository root is unavailable: ' + args.repository_root)
        result = validate(args.repository_root)
    except (OSError, UnicodeError) as error:
        print(json.dumps(dict(ok=False, error=str(error))))
        return 2
    if args.format == 'json':
        print(json.dumps(result, indent=2))
    else:
        print(f"{'PASS' if result['ok'] else 'FAIL'} roadmap: {result['milestones']} milestones, {result['workitems']} workitems")
        for diagnostic in result['diagnostics']:
            print(f"{diagnostic['code']} {diagnostic['path']}:{diagnostic['line']}: {diagnostic['message']}")
    return 0 if result['ok'] else 1


if __name__ == '__main__':
    sys.exit(main())
