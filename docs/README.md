
# or1n-rename-file-name-to-date-created

## 🔄 Status: Foundation In Progress (Phase 1: ~70% Complete)

**Phase 1 (Foundation & Core UI) is ~70% complete.** The app has window persistence (position, size, theme), theme system, and responsive UI foundation. WinUI 3 design system implementation is ongoing. Core rename functionality not yet started (Phase 2).

---

## Title

or1n Rename File Name To Date Created

## Short Description

A modern WinUI 3 desktop app (Windows 11) to batch rename files in a selected folder to their original date created/taken, with customizable formatting, prefix/suffix, and smart options. Features a clean, theme-aware UI with dark/light/system themes.

## Features

### Current Features ✅

- **Modern WinUI 3 UI** with responsive design and custom styling
  - Mica Alt backdrop with fallback for excellent Windows 11 integration
  - Responsive breakpoints (Compact < 720px, Medium 720-1079px, Wide > 1080px)
  - Entrance animations with proper staggered timing (0/40/80/160ms delays)

- **Professional Theme System**: Light, Dark, and System theme support
  - Centralized UIConfig.xaml with 40+ theme resources
  - Instant theme switching with no app restart
  - Custom title bar colors matching theme
  - Persistent theme preference across sessions

- **Complete Window State Persistence**
  - Position save/restore (X, Y coordinates)
  - Size save/restore (width, height)
  - Theme preference save/restore (Light/Dark)
  - Multi-monitor aware (prevents off-screen restoration)
  - Intelligent minimum size (710x640px ensures no UI cutoff)
  - JSON-based settings: `LocalAppData\Or1nRenameFileNameToDate\window-settings.json`

- **Performance & UX Polish**
  - Instant app close (200ms timeout, previously 5 seconds)
  - Debug logging for troubleshooting (`or1n-window-debug.log`)
  - Graceful error handling with silent fallback to defaults

- **Core UI Foundation**:
  - Folder picker for selecting target directory (UI only, no functionality yet)
  - File scanner placeholder (UI only, lists files by extension as demo)
  - Real-time logging of operations with timestamps
  - Clean, modern layout with title, subtitle, and controls

- **Customizable Appearance**:
  - Theme-aware colors (white/light gray for light theme)
  - Theme-aware colors (black/dark gray for dark theme)
  - Named sizing system for margins, padding, font sizes
  - Centered, wrapped subtitle for responsive text
  - Accessible button layout with 40x40px buttons

- **Accessibility & Compliance**
  - UseSystemFocusVisuals on all interactive controls
  - AutomationProperties.Name/HelpText for screen readers
  - IsTextScaleFactorEnabled for text scaling up to 225%
  - WCAG AA color contrast ratios
  - Keyboard navigation with proper tab order

### ❌ Not Yet Implemented (Planned)

The following features are **not yet implemented** and are tracked in [TODO.md](TODO.md):

- **Batch rename functionality**: Core renaming engine not yet built
- **File metadata extraction**: No image/video/file date reading (date created, date taken, EXIF, etc.)
- **Format configuration**: No customizable date/time formatting options
- **Prefix/suffix handling**: Not implemented
- **Bulk operations**: Cannot apply renames to multiple files
- **Preview before apply**: No dry-run or confirmation dialog
- **Error recovery**: Limited error handling and recovery options
- **Advanced filters**: No file type or date range filtering
- **Settings/profiles**: No save/load configuration

**See [TODO.md](TODO.md) for the detailed roadmap and tracking.**

Phase 2 (v1.1) planned items:

- ProgressRing for file operations
- Enhanced error messages and recovery
- More comprehensive code documentation

Phase 3 (v2.0) planned items:

- **Batch Rename Engine**: Date/time formatting, prefix/suffix, smart numbering
- **Advanced WinUI 3 Controls**: RichEditBox, NumberBox, ContentDialog, ProgressRing, InfoBar
- **File Filtering**: Select by file type, date range, size criteria
- **Preview & Results**: Inline file preview, rename preview, detailed results

Phase 4 (v2.1) planned items:

- Full keyboard navigation
- Screen reader optimization
- High contrast mode support

Phase 5+ (v3.0+) planned items:

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

**v2026.02.01.17.15.39.288** (Foundation Release: Core UI & Theme System)

## Accessibility

- Full screen reader and keyboard support
- High color contrast
- Accessible controls and layouts

## License

MIT
