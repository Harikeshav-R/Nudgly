# Agent Directives & Best Practices

*C# + Avalonia • Windows & macOS • Screenshare-Proof Overlay*

**Project Description:** This project is a standalone overlay application built with C# and Avalonia UI. It runs as its own process and renders a borderless, transparent, always-on-top window that sits over other applications. The UI is shared across Windows and macOS, with platform-specific native bindings handling screen capture exclusion. Linux is not supported.

---

## Core Architectural Approach: The Standalone Transparent Overlay

To deliver a screenshare-proof overlay, this project runs as a normal standalone executable that creates a borderless, transparent, always-on-top window and hides it from the OS capture pipeline using native APIs.

### How It Works

- **Launch as a Normal Process:** The application starts as a standard executable. No injection, hooking, or secondary process is involved.
- **Create a Transparent Always-On-Top Window:** On startup, Avalonia creates a borderless, transparent, click-through window configured to always sit above other windows. The window tracks the user's primary display (or a target window region, if applicable).
- **Hide from Screenshare:** Immediately after window creation, native OS APIs are called to exclude the window from the OS hardware compositor and all screen capture software (OBS, Discord, Snipping Tool, QuickTime, etc.).
- **Render the Overlay UI:** Avalonia renders the overlay content onto this window. Because the window is excluded at the compositor level, it is invisible to all capture tools but fully visible on the physical display.

| Platform | Capture Exclusion API | Binding Method |
|---|---|---|
| Windows | `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)` | Win32 P/Invoke |
| macOS | `NSWindow.sharingType = .none` | .NET macOS SDK (AppKit bindings) |

### Platform Project Structure

The solution is structured to maximise shared Avalonia UI code while isolating platform-specific native calls:

```
YourApp/
├── YourApp.Shared/        ← Avalonia UI, view models, shared logic
├── YourApp.Windows/       ← entry point, Win32 P/Invoke, WDA_EXCLUDEFROMCAPTURE
└── YourApp.macOS/         ← entry point, .NET macOS SDK, AppKit/NSWindow bindings
```

Each platform project is a standalone executable that references `YourApp.Shared` and registers its platform-specific service implementations at startup via the DI container.

### Why This Approach

- **Simplicity:** A standalone process is straightforward to build, distribute, and debug. No injection scaffolding, no cross-process communication.
- **Screenshare Immunity:** Leverages native OS compositor-level exclusion (`WDA_EXCLUDEFROMCAPTURE` / `sharingType = .none`) rather than fragile capture software hooking.
- **Clean Interop:** macOS uses first-class .NET macOS SDK AppKit bindings (no raw P/Invoke). Windows uses well-documented Win32 P/Invoke with a minimal surface area.
- **Avalonia Compatibility:** Avalonia renders reliably on the transparent surface we own and control.

### Window Configuration

The overlay window must be configured with the following properties at creation time, before it is shown:

- `SystemDecorations = None` — no title bar or borders
- `Background = Transparent`
- `TransparencyLevelHint = [TransparencySupport.Transparent]`
- `Topmost = true` — always-on-top
- `IsHitTestVisible = false` (or equivalent click-through configuration) — input passes through to the window beneath
- Capture exclusion API called **before** `Show()` to avoid a single frame appearing in captures

---

## 1. C# & Avalonia Best Practices

### Language & Code Quality

- **Idiomatic C#:** Write modern, idiomatic C# using current language features (nullable reference types, pattern matching, records, primary constructors where appropriate).
- **Static Analysis:** Enable and address all Roslyn analyser warnings. Use `.editorconfig` to enforce consistent style across the solution.
- **Nullability:** Enable nullable reference types project-wide (`#nullable enable`). Treat nullable warnings as errors in CI.
- **Error Handling:** Prefer Result-style patterns or custom exceptions over swallowing errors. Never silently catch `Exception` without logging. Avoid throwing generic `Exception` — use specific types.
- **Types & Encapsulation:** Use records for immutable data transfer objects. Prefer `sealed` classes where inheritance is not intended. Define clear interfaces for platform abstractions (e.g., `ICaptureExclusionService`).

### Avalonia UI Patterns

- **MVVM:** Strictly follow the MVVM pattern. Views contain no business logic. ViewModels expose only what the View needs.
- **CommunityToolkit:** Use CommunityToolkit for bindings, commands, and reactive properties.
- **Styles:** Define all visual styles in `.axaml` resource dictionaries. Do not hardcode colours or sizes in control markup.
- **Platform Abstraction:** Inject platform-specific services (e.g., `ICaptureExclusionService`) via the DI container. The shared project must never contain `#if WINDOWS` or `#if MACOS` directives — that logic belongs in the platform projects.

### Testing

- Write unit tests for all ViewModel logic and shared business logic using xUnit.
- Integration tests must cover the platform service implementations on their respective platforms.
- Tests must be deterministic and must not rely on external mutable state or real window handles.

---

## 2. Git Workflow & Source Control

All agents must adhere to a strict branching and commit workflow to ensure code quality and traceability.

### 2.1 Branching Strategy

- **Never Push to Main:** Direct pushes to the `main` branch are strictly prohibited. All changes must go through a Pull Request.
- **Branch Naming:** Branch names MUST use a `type/description` format.
  - *Examples:* `feat/overlay-opacity-control`, `fix/capture-exclusion-macos`, `refactor/platform-abstraction`, `docs/window-configuration`

### 2.2 Commit Guidelines

- **Conventional Commits:** You must strictly follow the [Conventional Commits](https://www.conventionalcommits.org/) format for all commit subjects.
  - *Examples:* `feat(overlay): implement WDA_EXCLUDEFROMCAPTURE on Windows`, `fix(macos): correct NSWindow sharingType binding`
- **Commit Bodies (Mandatory):** Every commit MUST include a descriptive subject line AND a detailed, multiline commit description.
  - The body should explain the *why* and *how* of the change, not just the *what*.
  - If a commit contains multiple logical changes or touches several components, use bullet points in the description body to detail each change.
- **Pull Requests:** Always open Pull Requests for review. PR descriptions must accurately summarise the changes and link to any relevant issue tracking numbers.

---

## 3. Mandatory Detailed Logging

Detailed, structured logging is a project-wide requirement. Visibility into overlay lifecycle and platform interop behaviour is critical.

- **Action Tracking:** Every significant action, state change, or platform interop call MUST be logged.
- **Structured Logging:** Use `Microsoft.Extensions.Logging` with a structured logging provider (e.g., Serilog) for all logging. Use `LoggerMessage` source-generated methods for high-performance hot paths.
- **Log Levels:** Use `Information` for lifecycle events, `Warning` for recoverable issues, `Error` for failures, and `Debug`/`Trace` for platform interop details.
- **Prohibition on Console Output:** Do not use `Console.WriteLine`, `Console.Error.WriteLine`, or `Debug.WriteLine` for operational logging. These are strictly for temporary local debugging and must be removed before committing.
- **Contextual Logs:** Include relevant context (e.g., window handle, process ID, platform, overlay state) in log events to facilitate debugging.

---

## 4. Platform-Specific Implementation Notes

### Windows — Win32 P/Invoke

The Windows platform project handles capture exclusion via a minimal, well-typed P/Invoke surface:

```csharp
internal static partial class NativeMethods
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    internal const uint WDA_NONE               = 0x00000000;
    internal const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
}
```

Capture exclusion must be applied immediately after the Avalonia window is initialised, before the first render:

```csharp
public class WindowsCaptureExclusionService : ICaptureExclusionService
{
    public void ExcludeFromCapture(Window window)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle
            ?? throw new InvalidOperationException("Could not obtain HWND");

        if (!NativeMethods.SetWindowDisplayAffinity(hwnd, NativeMethods.WDA_EXCLUDEFROMCAPTURE))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }
}
```

- Use `LibraryImport` (source-generated P/Invoke) over `DllImport` for new code — it is AOT-safe and avoids runtime marshalling overhead.
- Wrap all P/Invoke calls in a typed service implementing `ICaptureExclusionService`. Never call P/Invoke directly from ViewModels or Avalonia code.

### macOS — .NET macOS SDK (AppKit Bindings)

The macOS platform project uses first-class AppKit bindings from the .NET macOS SDK workload. No raw P/Invoke is required for `NSWindow` interop:

```csharp
using AppKit;

public class MacOSCaptureExclusionService : ICaptureExclusionService
{
    public void ExcludeFromCapture(Window window)
    {
        var nsWindow = window.TryGetPlatformHandle()?.Handle is { } handle
            ? ObjCRuntime.Runtime.GetNSObject<NSWindow>(handle)
            : throw new InvalidOperationException("Could not obtain NSWindow");

        nsWindow.SharingType = NSWindowSharingType.None;
    }
}
```

- All AppKit calls must occur on the main thread. Use `Dispatcher.UIThread.InvokeAsync` from Avalonia if calling from a background thread.
- Screenshot capture on macOS uses `ScreenCaptureKit` via the .NET macOS SDK — no P/Invoke required.

---

*Windows & macOS only • No Linux support • C# + Avalonia + .NET Platform SDKs*
