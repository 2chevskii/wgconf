---
description: Format, stage, and commit changes following WgConf project conventions
argument-hint: [commit message hint or type]
---

You are making a git commit in the WgConf repository. Follow these steps in order.

## Arguments

The user provided: "$ARGUMENTS"

Use this as a hint for the commit message or type if provided. If empty, infer everything from the diff.

## Step 1: Assess Current State

Run in parallel:
- `git status` to see staged, unstaged, and untracked files
- `git diff HEAD` to understand the full scope of changes

## Step 2: Run CSharpier Formatter

Run `dotnet csharpier format .` to auto-format all C# source files.

If it fails (tools not restored), run `dotnet tool restore` first, then retry.

After formatting, run `git diff --name-only` to identify any files CSharpier changed — stage them with the rest.

## Step 3: Stage Files

Stage relevant files following these rules:

- Stage source files from `src/` and `tests/` that are part of the change
- Stage any files CSharpier reformatted
- Stage documentation (`*.md`, `.claude/*.md`) or CI (`.github/`) changes if they are part of the work
- **Do NOT stage `Version.props`** unless the user explicitly asked to prepare a release
- **Do NOT stage** build artifacts (`bin/`, `obj/`, `TestResults/`, `coverage-report/`)
- Prefer specific file paths over `git add .`

## Step 4: Determine the Commit Message

Format: `<type>[optional scope]: <description>`

**Types**: `feat`, `fix`, `test`, `chore`, `ci`, `docs`

**Scope**: Use `(awg)` only when changes are exclusively in `src/WgConf.Amnezia/` or `tests/WgConf.Amnezia.Tests/`

**Description**: Imperative mood, no period, under 72 characters.

**Do NOT use `+semver:` hints** — versioning is managed manually via `Version.props`.

## Step 5: Create the Commit

Create the commit. Do not push unless explicitly asked.

Show `git log --oneline -1` so the user can confirm.
