# Template Validation with Detailed Diff

## Problem
Template validation was failing with just "content doesn't match" - not helpful for fixing the issue.

## Solution
Enhanced `TemplateValidationResult` with a `DetailedDiff` property that provides character-level diff information.

## How It Works

### 1. Enhanced TemplateValidationResult

Added `DetailedDiff` property that contains:
- Expected vs Actual lengths
- Position of first difference
- Character codes at difference point
- Context around the difference (40 chars before/after)
- Full expected and actual output

### 2. Automatic Diff Generation

When `TemplateValidationResult.Mismatch()` is called, it automatically builds a detailed diff if there's a Content mismatch:

```csharp
public static TemplateValidationResult Mismatch(
    TemplateResult actual,
    Dictionary<string, (string Expected, string Actual)> differences) =>
    new()
    {
        IsValid = false,
        ErrorMessage = $"Validation failed with {differences.Count} mismatch(es)",
        ActualResult = actual,
        Differences = differences,
        DetailedDiff = BuildDetailedDiff(differences) // ? Automatic diff
    };
```

### 3. Example Output

When a template fails validation, you get:

```
===== CONTENT DIFF =====
Expected Length: 245
Actual Length: 267

First difference at position 89:
  Expected char: '}'  (code: 125)
  Actual char: '\n' (code: 10)

Expected context:
  ...name:·string;\n··age:·number;\n}\n\nexport·const·UserCard...

Actual context:
  ...name:·string;\n··age:·number;\n\n}\n\nexport·const·UserCard...

===== FULL EXPECTED =====
import React from 'react';

export interface UserCardProps {
  name: string;
  age: number;
}

export const UserCard...

===== FULL ACTUAL =====
import React from 'react';

export interface UserCardProps {
  name: string;
  age: number;

}

export const UserCard...

===== END DIFF =====
```

## Usage

### In Tests

Debug tests now show the diff automatically:

```csharp
[Fact]
public void ShowValidationResult()
{
    if (!_validationResult!.IsValid)
    {
        Console.WriteLine(_validationResult.DetailedDiff);
    }
    
    _validationResult.IsValid.Should().BeTrue();
}
```

### In Code

Access the diff programmatically:

```csharp
var template = new CSharpClassTemplate();
var result = template.ValidateDetailed();

if (!result.IsValid && result.DetailedDiff != null)
{
    Console.WriteLine(result.DetailedDiff);
    // Now you know exactly what to fix!
}
```

## Benefits

1. **See exact differences** - Character position, codes, context
2. **Fix quickly** - No guessing what's wrong
3. **Adjust correctly** - Either fix template or update ExpectedContent
4. **Prevent drift** - Byte-for-byte validation with helpful feedback

## Special Character Handling

The diff escapes special characters for clarity:
- `\n` ? visible as `\n`
- `\r` ? visible as `\r`
- `\t` ? visible as `\t`
- ` ` (space) ? visible as `·` (middle dot)

## Using the Diff to Fix Templates

When you see a diff:

1. **Extra newlines?** ? Common with `@foreach` blocks
   - Check if template has blank lines after `}`
   - Update `ExpectedContent` to match

2. **Different characters?** ? Variable interpolation issue
   - Check variable resolution
   - Verify data in `TestDataJson`

3. **Wrong length?** ? Missing or extra content
   - Check conditional blocks (`@if`)
   - Verify all loops execute

## Example: Fixing the ComponentTemplate

**Diff shows:**
```
First difference at position 89:
  Expected char: '}' (code: 125)
  Actual char: '\n' (code: 10)
```

**Analysis:** There's an extra newline after the props loop.

**Fix:** Update `ExpectedContent` to include that newline:
```csharp
public override string ExpectedContent => @"
export interface UserCardProps {
  name: string;
  age: number;

}  // ? Extra blank line here
```

## Summary

- ? **Detailed diffs** show exact character-level differences
- ? **Context** helps understand where differences occur
- ? **Full output** lets you verify the complete result
- ? **Automatic** - built into `TemplateValidationResult.Mismatch()`
- ? **Helpful** - makes fixing templates trivial

No more guessing! The diff tells you exactly what needs to change.
