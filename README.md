
# or1n-rename-file-name-to-date-created

## 🔄 Status: Foundation Complete (Phase 1: 100% Complete)

**Phase 1 (Foundation & Core UI) is 100% complete.** All core UI features, window persistence, theme system, and console output with line-height snapping and auto-scroll are fully functional. WinUI 3 design system implementation is complete. Core rename functionality is planned for Phase 2 (v2.0+). See [docs/TODO.md](docs/TODO.md) for the development roadmap.

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
- **Professional Theme System**: Light, Dark, and System theme support
  - Theme-aware colors and instant switching
  - Custom title bar colors matching theme
  - Persistent theme preference across sessions
- **Complete Window State Persistence**
  - Position, size, and theme preference save/restore
  - Multi-monitor aware (prevents off-screen restoration)
  - Intelligent minimum size ensures no UI cutoff
- **Professional Console Output**
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

### ❌ Not Yet Implemented (Planned)

The following features are **not yet implemented** and are tracked in [docs/TODO.md](docs/TODO.md):

- **Batch rename functionality**: Core renaming engine not yet built
- **File metadata extraction**: No image/video/file date reading (date created, date taken, EXIF, etc.)
- **Format configuration**: No customizable date/time formatting options
- **Prefix/suffix handling**: Not implemented
- **Bulk operations**: Cannot apply renames to multiple files
- **Preview before apply**: No dry-run or confirmation dialog
- **Error recovery**: Limited error handling and recovery options
- **Advanced filters**: No file type or date range filtering
- **Settings/profiles**: No save/load configuration

**See [docs/TODO.md](docs/TODO.md) for the detailed roadmap and tracking.**

### Phase 2 (v1.1) Planned Items

- ProgressRing for file operations
- Enhanced error messages and recovery
- More comprehensive code documentation

### Phase 3 (v2.0) Planned Items

- **Batch Rename Engine**: Date/time formatting, prefix/suffix, smart numbering
- **Advanced WinUI 3 Controls**: RichEditBox, NumberBox, ContentDialog, ProgressRing, InfoBar
- **File Filtering**: Select by file type, date range, size criteria
- **Preview & Results**: Inline file preview, rename preview, detailed results

### Phase 4 (v2.1) Planned Items

- Full keyboard navigation
- Screen reader optimization
- High contrast mode support

### Phase 5+ (v3.0+) Planned Items

- Save/load rename profiles
- Undo/redo support
- Settings and preferences page
- Advanced animations and visual effects

## Requirements

- **Windows 11** (Build 19041 or later)
- **.NET SDK 8.0** or later
- **Windows App SDK 1.8** or later (automatically installed via NuGet)
- **VS Code** (recommended) or Visual Studio 2022

## Installation

### Using VS Code (Recommended)

1. Clone or download this repository
2. Open the folder in VS Code
3. Install recommended extensions:
   - C# Dev Kit
   - XAML Language Support
   - .NET Runtime
4. Open a terminal and run:

   ```bash
   dotnet restore
   dotnet build
   ```

### Using Visual Studio 2022

1. Clone or download this repository
2. Open `or1n-rename-file-name-to-date-created.slnx` in Visual Studio
3. Wait for NuGet packages to restore
4. Build the solution (Ctrl+Shift+B)
5. Run the app (F5)

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
