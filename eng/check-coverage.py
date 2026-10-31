#!/usr/bin/env python3
"""Fail a build when test coverage drops below the recorded floor.

Coverage is only a gate when something reads the number and exits non-zero. This
script reads the Cobertura reports `dotnet test --collect:"XPlat Code Coverage"`
writes and compares the line rate against the floor the workflow passes in.

Usage:
    python eng/check-coverage.py <results-directory> --minimum 70
"""

import argparse
import glob
import os
import sys
import xml.etree.ElementTree as ElementTree


def read_totals(directory):
    reports = sorted(glob.glob(os.path.join(directory, "**", "coverage.cobertura.xml"), recursive=True))
    if not reports:
        return None, 0, 0, []
    covered = 0
    valid = 0
    for report in reports:
        root = ElementTree.parse(report).getroot()
        covered += int(root.get("lines-covered", "0"))
        valid += int(root.get("lines-valid", "0"))
    return (covered / valid) if valid else 0.0, covered, valid, reports


def main():
    parser = argparse.ArgumentParser(description="Enforce the recorded line-coverage floor.")
    parser.add_argument("directory", help="directory searched for coverage.cobertura.xml")
    parser.add_argument("--minimum", type=float, required=True, help="minimum line coverage percentage")
    arguments = parser.parse_args()

    rate, covered, valid, reports = read_totals(arguments.directory)
    if rate is None:
        print("::error::No coverage.cobertura.xml under %s - the coverage gate cannot be evaluated." % arguments.directory)
        return 2

    percentage = rate * 100
    print("coverage %.2f%% (%d/%d lines) across %d report(s); floor %.2f%%" % (percentage, covered, valid, len(reports), arguments.minimum))
    if percentage < arguments.minimum:
        print("::error::Coverage %.2f%% is below the recorded floor of %.2f%%." % (percentage, arguments.minimum))
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
