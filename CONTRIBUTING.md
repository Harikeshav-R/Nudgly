# Contributing

Thank you for your interest in contributing to this project!

## Code Structure

This project is built with C# and Avalonia UI. It provides a transparent overlay application for Windows and macOS. Linux is not supported.

- `YourApp.Shared/`: Avalonia UI, view models, and shared logic. NO `#if WINDOWS` or `#if MACOS` compiler directives should be placed here.
- `YourApp.Windows/`: Windows entry point and Win32 P/Invoke interop (`WDA_EXCLUDEFROMCAPTURE`).
- `YourApp.macOS/`: macOS entry point and AppKit bindings.

## Git Workflow

We strictly follow a structured Git workflow:

1. **Branch Naming**: Branch names must use a `type/description` format (e.g., `feat/overlay-opacity-control`, `fix/capture-exclusion-macos`).
2. **Never Push to Main**: All changes must go through a Pull Request.
3. **Conventional Commits**: Commit subjects must follow the [Conventional Commits](https://www.conventionalcommits.org/) format. (e.g., `feat(overlay): implement WDA_EXCLUDEFROMCAPTURE on Windows`).
4. **Commit Bodies**: Every commit must include a detailed multiline commit description explaining the *why* and *how*.

## C# & Avalonia Best Practices

- Use modern, idiomatic C# with `#nullable enable`.
- Strictly follow the MVVM pattern (with `ReactiveUI`).
- Use `.axaml` resource dictionaries for styling instead of hardcoding values.
- **Logging**: Use structured logging via `Microsoft.Extensions.Logging`. Never use `Console.WriteLine` or `Debug.WriteLine`.

## Pull Requests

1. Fork the repo and create your branch from `main`.
2. Ensure any new code has unit tests.
3. Update documentation if necessary.
4. Make sure your PR passes all CI checks.
