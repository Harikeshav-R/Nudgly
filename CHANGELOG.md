# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.1] - 2026-03-12

### Added
- Initial project structure.
- GitHub issue templates and PR templates.
- Security and contributing guidelines.
- CI/CD workflows for building, testing, and releasing.
- Cross-platform screen capture services for Windows and macOS.
- Screenshare-proof transparent overlay window setup with compositor-level capture exclusion.
- Centralized semantic version management via `Directory.Build.props`.

### Changed
- Migrated Windows screen capture from `System.Drawing` to native Win32 GDI APIs (via `CsWin32`) to enable Native AOT compatibility.

### Fixed
- Resolved unmanaged memory leaks in macOS and Windows platform services by implementing proper `IDisposable` patterns and try/finally lifecycle blocks.
- Resolved native API interop vulnerabilities during Windows screen capture operations.
