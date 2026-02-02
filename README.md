
# or1n-rename-file-name-to-date-created

## 🔄 Status: Core UI Ready

**Core UI features, window persistence, theming, and console output are fully functional.** Batch rename functionality and advanced controls are still in progress. See [docs/TODO.md](docs/TODO.md) for the current roadmap.

## Title

or1n Rename File Name To Date Created

## Short Description

A modern WinUI 3 desktop app (Windows 11) to batch rename files in a selected folder to their original date created/taken, with customizable formatting, prefix/suffix, and smart options. Features a clean, theme-aware UI with dark/light/system themes.

## Features

### Current Features ✅

- **Modern WinUI 3 UI** with responsive design and custom styling
  - DesktopAcrylic frosted glass with Mica Alt fallback
  - Responsive breakpoints and entrance animations
  - Consistent motion system for hover/press feedback
  - Keyboard navigation with verified tab order
  - Polished controls (consistent sizing, spacing, corner radius)
- **Theme System**: Light, Dark, and System theme support
  - Theme-aware colors and instant switching
  - Custom title bar colors matching theme
  - Persistent theme preference across sessions
- **Complete Window State Persistence**
  - Position, size, and theme preference save/restore
  - Multi-monitor aware (prevents off-screen restoration)
  - Intelligent minimum size ensures no UI cutoff
- **Console Output**
  - Real-time operation logging with timestamps
  - Theme-aware coloring (dark text in light mode, bright text in dark mode)
  - Right-click context menu (Select All, Copy, Copy All)
  - Line-height snapping (no partial lines) with full scrollback
  - Always auto-scrolls to the newest entry on new log output
  - Color-coded messages by log level (Info, Warning, Error, Success, Debug)
- **Core UI Foundation**:
  - Custom WinUI 3 folder picker window with theme-matched title bar
  - Folder picker remembers last path, size, and position
  - File scanner UI showing files by extension (demo ready)
  - Clean, modern responsive layout
  - Accessible button controls

### ❌ Not Yet Implemented

The core rename engine and advanced controls are still in progress. See [docs/TODO.md](docs/TODO.md) for the current work list and priorities.

## Requirements

- **Windows 11** (Build 19041 or later)
- **.NET SDK 8.0** or later
- **Windows App SDK 1.8** or later (automatically installed via NuGet)
- **VS Code** (recommended) or Visual Studio 2022

## Installation & Workflow

- Setup instructions: [docs/SETUP.md](docs/SETUP.md)
- Build/run workflow: [docs/WORKFLOW.md](docs/WORKFLOW.md)

## Usage

1. **Launch the app**: Press F5 in VS Code or click Run in Visual Studio
2. **Select a folder**: Click "Open" button to choose a directory to scan
3. **Select theme**: Choose between "System theme", "Light theme", or "Dark theme" from the dropdown
4. **Scan files**: Click "Scan" button to analyze files in the selected folder
5. **View results**: Check the log area at the bottom for operation details

## Building from Command Line

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the app
dotnet run --project or1n-rename-file-name-to-date-created.csproj

# Watch mode (rebuilds on file changes)
dotnet watch run --project or1n-rename-file-name-to-date-created.csproj
```

## Versioning

This project uses **timestamp-based versioning** for simplicity and reproducibility:

**Format**: `v{YYYY}.{MM}.{DD}.{HH}.{MM}.{SS}.{MSS}`

### Example

- `v2026.02.01.17.15.39.288` - Released on Feb 1, 2026 at 17:15:39.288 UTC

### Benefits

- ✅ Automatic versioning (no manual bumps needed)
- ✅ Timestamp-sorted versions (easy chronological ordering)
- ✅ Build reproducibility with millisecond precision
- ✅ Clear commit timeline (when was each change made)

### Current Version

**v2026.02.02.03.05.000** (Motion System & Control Polish Complete)

## Accessibility

- Full screen reader and keyboard support
- High color contrast
- Accessible controls and layouts

## License

MIT
