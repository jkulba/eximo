# Eximo

Eximo (Latin: *to remove, release, or exempt*) is a .NET 10 console application that bridges legacy application logging into structured, enriched log output via Serilog.

It accepts JSON-formatted log messages from legacy systems on the command line, validates them, enriches them with runtime environment metadata, and emits them as structured log events.

---

## Features

- Accepts legacy log payloads as JSON over the CLI
- Validates payloads with [FluentValidation](https://fluentvalidation.net/)
- Enriches every log event with:
  - **OS** — detected operating system (`Windows`, `Linux`, `macOS`)
  - **DotNet** — all installed .NET runtimes found on the host
  - **Version** — application build number from `version.json`
- Forwards enriched events to Serilog (console and file sinks)
- Returns structured exit codes for downstream automation

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## Building

```bash
dotnet build src/App/App.csproj
```

To publish a self-contained executable:

```bash
dotnet publish src/App/App.csproj -c Release
```

---

## Usage

```
eximo log <json-payload>
```

### JSON Payload Format

| Field        | Type                  | Required | Constraints                                      |
|--------------|-----------------------|----------|--------------------------------------------------|
| `message`    | string                | Yes      | Max 10,000 characters                            |
| `level`      | string                | Yes      | One of: `Debug`, `Information`, `Warning`, `Error`, `Critical` (case-insensitive) |
| `application`| string                | Yes      | Max 100 characters                               |
| `source`     | string                | No       | Max 200 characters                               |
| `timestamp`  | ISO 8601 datetime     | No       | Must not be more than 5 minutes in the future; defaults to current UTC time |
| `properties` | object (string keys)  | No       | Arbitrary key/value pairs added to the log scope |

### Example

```bash
eximo log '{
  "level": "Warning",
  "message": "Disk usage exceeded threshold",
  "application": "InventoryService",
  "source": "DiskMonitor",
  "timestamp": "2026-05-06T12:00:00Z",
  "properties": {
    "diskUsagePercent": 92,
    "drive": "C:"
  }
}'
```

---

## Exit Codes

| Code | Constant           | Meaning                              |
|------|--------------------|--------------------------------------|
| `0`  | `Success`          | Log event processed successfully     |
| `1`  | `InvalidJson`      | Payload could not be parsed as JSON  |
| `2`  | `ValidationFailed` | Payload failed validation rules      |
| `99` | `UnexpectedError`  | An unhandled exception occurred      |

---

## Configuration

Application settings are loaded from `appsettings.json` in the executable directory. Serilog sinks (console, file) and minimum log levels are configured there.

The application build number is read from `version.json` in the executable directory:

```json
{
  "BuildNumber": "1.0.0"
}
```

If `version.json` is absent or malformed, the version is reported as `unknown`.

---

## Release versioning

### How the build number is calculated

Tags are created by CI only when a push lands on a `release/*` branch. The build number is derived directly from the branch name — no external file is read.

```
<branch-name-without-release-prefix>-<github.run_id>
```

For example, a push to `release/202605_R1` with run ID `25470999999` produces:

```
202605_R1-25470999999
```

### What the pipeline does with the build number

1. **Writes `src/App/version.json`** — populates `BuildNumber`, `GitHead`, `BuildDate`, `GitBranch`, `GitTag`, and `CommitHash` with values from the current run.
2. **Commits `version.json` back to the release branch** — committed by `github-actions[bot]` so the built artifact always carries traceable version metadata.
3. **Creates and pushes a git tag** — permanently marks the exact commit that produced the release artifacts.

---

## GitHub Actions Workflows

### CI (`ci.yml`)

Triggered automatically on every push or pull request to `main`, `dev`, or any `release/*` branch. Can also be triggered **on demand** from the GitHub Actions UI against any branch. Runs build and test with coverage on all branches. The **tag** and **package** jobs run only on pushes to `release/*` branches.

**How to trigger manually from the GitHub web portal:**

1. Navigate to the repository on GitHub.
2. Click **Actions** in the top navigation bar.
3. Select **Build Eximo App** from the left-hand workflow list.
4. Click **Run workflow**.
5. Select the branch you want to build from the dropdown.
6. Click the green **Run workflow** button.

---

### Create Release Branch (`create-release-branch.yml`)

Creates a `release/{branch_name}` branch from an existing git tag. Both the source tag and the desired branch name are provided explicitly by the operator.

**When to use:** After CI has tagged a build from a release branch and you need a new dedicated branch — for example for a hotfix or a parallel stabilisation stream.

**How to run from the GitHub web portal:**

1. Navigate to the repository on GitHub.
2. Click **Actions** in the top navigation bar.
3. Select **Create Release Branch** from the left-hand workflow list.
4. Click **Run workflow**.
5. Enter the full **Git tag** (e.g. `202605_R1-25470635259`).
6. Enter the **Branch name** without the `release/` prefix (e.g. `202605_R1` or `202605_R1_hotfix1`).
7. Click the green **Run workflow** button.

The workflow creates and pushes `release/202605_R1` from the exact commit the tag points to.

---

### Rebase Branch onto Tag (`rebase-branch-onto-tag.yml`)

Rebases a target branch onto a given git tag. The workflow handles two distinct cases automatically:

| Scenario | What happens |
|---|---|
| Target branch is behind or at the tag (e.g. `main`) | Branch is reset to the tag — fast-forward, no replay needed |
| Target branch has commits ahead of the tag (e.g. `dev`) | Unique commits are replayed on top of the tag via rebase |

This is **forward integration** — the established CM practice of re-rooting a working branch onto a new released baseline before the next development cycle begins.

**When to use:**
- After a release tag is finalised, run with `target_branch=main` to advance `main` to the tag without a merge commit or PR.
- After advancing `main`, run with `target_branch=dev` to replay any in-flight `dev` commits on top of the new baseline.

> **Conflict note:** If the rebase cannot auto-resolve conflicts, the workflow aborts and leaves the branch untouched. Resolve the conflicts locally (`git rebase <tag>`), then re-run the workflow.

**How to run from the GitHub web portal:**

1. Navigate to the repository on GitHub.
2. Click **Actions** in the top navigation bar.
3. Select **Rebase Branch onto Tag** from the left-hand workflow list.
4. Click **Run workflow**.
5. Enter the full **Git tag** (e.g. `202605_R1-25470635259`).
6. Enter the **Target branch** (`main` or `dev`).
7. Click the green **Run workflow** button.

The workflow uses `--force-with-lease` on push, which protects against overwriting concurrent pushes to the target branch.

**Complete release cycle:**

```
─── Stabilization ───────────────────────────────────────────────────
  Developers open PRs → release/202605_R1
  CI on release/* push → tag: 202605_R1-25470999999
                       → updates version.json on release branch

─── Release ─────────────────────────────────────────────────────────
  rebase-branch-onto-tag
    git_tag=202605_R1-25470999999  target_branch=main
    → main reset to tag (fast-forward, no merge commit)

─── Forward Integration ─────────────────────────────────────────────
  rebase-branch-onto-tag
    git_tag=202605_R1-25470999999  target_branch=dev
    → dev-only commits replayed on top of tag

  Developer commits to dev: open next cycle
```

---

## Architecture

```
Program.cs
  └── CoconaApp
        ├── Serilog (with enrichers)
        │     ├── OsEnricher          — adds "OS" property
        │     ├── DotNetRuntimesEnricher — adds "DotNet" property
        │     └── VersionEnricher     — adds "Version" property
        └── LogCommands
              └── log <json>
                    ├── JSON parse
                    ├── FluentValidation (LogMessageValidator)
                    └── ILogger.Log() with scoped properties
```

**Key components:**

- [LogCommands.cs](src/App/LogCommands.cs) — CLI command handler; parses, validates, and logs the payload
- [LogMessage.cs](src/App/LogMessage.cs) — payload model, validator, and error code constants
- [OsEnricher.cs](src/App/OsEnricher.cs) — Serilog enricher for OS detection
- [DotNetRuntimesEnricher.cs](src/App/DotNetRuntimesEnricher.cs) — Serilog enricher that enumerates installed .NET runtimes
- [VersionEnricher.cs](src/App/VersionEnricher.cs) — Serilog enricher that reads the build number from `version.json`

---

## Testing

Tests are written with [xUnit v3](https://xunit.net/) and target .NET 10.

```bash
dotnet test tests/App.Tests/App.Tests.csproj
```

Test coverage includes:

- `LogMessageValidator` — 20 cases covering all validation rules for every field
- `LogCommands.LogAsync` — 8 cases covering JSON parsing, validation errors, and all log levels
- `OsEnricher` — verifies OS property is added and contains a known platform value
- `DotNetRuntimesEnricher` — verifies DotNet property is added as a collection
- `VersionEnricher` — verifies Version property is added as a non-empty scalar
