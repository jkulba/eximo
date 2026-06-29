# Copilot Instructions for Eximo

## Project Overview

Eximo (Latin: *to remove, release, or exempt*) is a .NET 10 C# application structured as a solution with a main application project and a separate test project.

- **Solution:** `App.slnx`
- **Runtime:** .NET 10 (`net10.0`)
- **Language:** C# with nullable reference types and implicit usings enabled
- **Test framework:** xUnit v3 (`xunit.v3.mtp-v2`)

### Project Structure

```
src/
  App/           # Main application
tests/
  App.Tests/     # xUnit test project
```

## Coding Conventions

- Use C# 13+ language features where appropriate
- Nullable reference types are enabled — always handle nullability explicitly
- Follow standard .NET naming conventions (PascalCase for types and members, camelCase for locals)
- Keep `Program.cs` minimal; extract logic into well-named classes and methods
- Place tests in `App.Tests` mirroring the namespace structure of `App`

## Testing

- Write xUnit v3 facts and theories for all non-trivial logic
- Each test should have a single, clear assertion
- Name tests using the pattern: `MethodName_Scenario_ExpectedResult`

## Git & Contributions

- Do **not** add GitHub Copilot, any AI model, or any AI tool as a co-author or contributor in generated commit messages, pull request descriptions, or any other git actions.
- Commit messages should attribute only the human author(s) involved in the change.
- Do not append `Co-authored-by: GitHub Copilot` or similar lines to any generated output.

## Techne Skills

This project uses the techne skills library for release automation.
Skills are installed at `~/.techne/skills/`.

When performing git release operations, read the relevant skill file before executing:
- Release branching: `~/.techne/skills/git-release-manager.md`
- Cherry-pick selection: `~/.techne/skills/git-release-cherry-pick.md`
- Promotion gates: `~/.techne/skills/git-release-promote.md`
- Dev sync: `~/.techne/skills/git-sync-dev.md`
- Commit messages: `~/.techne/skills/git-commit-message.md`

The skill manifest is at `~/.techne/manifests/skills.json`.