# Changelog

**Current Version: v2026.02.02.06.52.30.267**

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project uses timestamp-based versioning in the format `v{YYYY}.{MM}.{DD}.{HH}.{mm}.{ss}.{fff}`.

## [v2026.02.02.06.52.30.267] - 2026-02-02 - Scan UI Responsiveness & Progress Updates

### Added (v2026.02.02.06.52.30.267)

- **Scan wait cursor** — Pointer switches to wait cursor while scans run

### Changed (v2026.02.02.06.52.30.267)

- **Log output buffering** — Batched log updates to keep the UI responsive during large scans
- **Progress updates** — Throttled progress bar updates to prevent UI stalls
- **UI thread marshaling** — Scan progress/log updates safely dispatched to UI thread

### Technical Details (v2026.02.02.06.52.30.267)

- **Log queue**: Concurrent queue with a 200ms dispatcher flush
- **Progress throttling**: UI updates limited to ~100ms cadence
- **Cursor**: `ProtectedCursor` set to Wait/Arrow during scans

---

## [v2026.02.02.06.27.26.345] - 2026-02-02 - Scan Filters & Log Scrollback

### Added (v2026.02.02.06.27.26.345)

- **Scan filter dialog** — Select file types to include before starting a metadata scan
- **Column-based file type selector** — 10 items per column, auto-expands horizontally

### Changed (v2026.02.02.06.27.26.345)

- **Log scrollback** — Removed log entry cap and enabled full scroll history
- **Scan workflow** — Scan begins only after file types are confirmed

### Technical Details (v2026.02.02.06.27.26.345)

- **ScrollViewer**: Vertical scroll bar set to Auto for log output
- **Filtering**: Metadata scan now accepts extension filters for precise selection

---

## [v2026.02.02.06.19.24.458] - 2026-02-02 - Metadata Scan Pipeline

### Added (v2026.02.02.06.19.24.458)

- **MetadataScanService** — Centralized metadata scanning with EXIF for images, media tags for audio/video, and file system fallback
- **Scan button pipeline** — Scan now extracts filename, extension, file size, and date components for each file
- **File type categorization** — Images, videos, audio, documents, executables, binaries, archives, and other
- **Structured scan output** — Detailed per-file log lines plus category/date-source summaries

### Changed (v2026.02.02.06.19.24.458)

- **Scan performance** — Parallelized file processing with CPU-aware throttling
- **Date selection logic** — Prioritizes DateTaken → MediaTaggedDate → DateCreated for consistent results

### Technical Details (v2026.02.02.06.19.24.458)

- **Libraries**: MetadataExtractor 2.9.0, TagLibSharp 2.3.0
- **Parallel scan**: Uses `Parallel.ForEach` with `MaxDegreeOfParallelism = CPU - 1`
- **Date formatting**: `yyyy-MM-dd HH:mm:ss.fff` with explicit parts logged per file

---

## [v2026.02.02.05.58.437] - 2026-02-02 - Application Infrastructure & Version Service

### Added (v2026.02.02.05.58.437)

- **VersionService utility class** — Dynamic version generation using system time with format `v{YYYY}.{MM}.{DD}.{HH}.{mm}.{ss}.{fff}` (e.g., v2026.02.02.05.58.437)
- **Application version display** — Version shown at bottom right of main window ("version 2026.02.02.XX.XX.XX.XXX") with real-time timestamp
- **GitHub repository link** — Clickable "github page" text at bottom left that opens https://github.com/or1n/or1n-rename-file-name-to-date-created

### Changed (v2026.02.02.05.58.437)

- **Footer layout** — New Grid-based footer with GitHub link (left), spacer (center), and version display (right)
- **Version string format** — Removed "v" prefix and colon from UI display (shows "version 2026.02.02.XX.XX.XX.XXX" instead of "Version: vXXXX...")

### Technical Details (v2026.02.02.05.58.437)

- **VersionService**: Static utility in `Helpers/VersionService.cs` with `GetCurrentVersion()` method
- **Dynamic versioning**: Retrieved at app startup; always shows actual system time when app launches
- **GitHub link behavior**: Uses `Windows.System.Launcher.LaunchUriAsync()` to open URI in default browser
- **GitHub link interaction**: Scales 1.0→1.05 on hover and 1.0 on exit for visual feedback
- **Footer styling**: Uses AccentBrush for GitHub link, LogTimestampBrush for version text

---

## [v2026.02.02.05.40.892] - 2026-02-02 - Folder Picker UX Comprehensive Improvements

### Added (v2026.02.02.05.40.892)

- **Inline path editing** — Path text now editable inline; TextBox replaces TextBlock in the same grid location when clicked
- **Drives/This PC button** — New quick access button to browse and select from all available drives via dialog
- **Select Drive dialog** — Modal dialog listing all ready drives with drive name and volume label
- **Hover visual feedback** — Path text opacity changes (70%) on hover to indicate it's clickable
- **Fixed quick access button sizing** — Buttons now 48x42 with proper icon sizing (18px) for better visibility

### Changed (v2026.02.02.05.40.892)

- **Select Drive dialog theme** — Dialog respects light/dark theme matching the folder picker window theme
- **Select Drive button spacing** — Improved padding and spacing in drive list buttons
- **Navigation bar icons** — Up button also increased to 48x42 for consistency with quick access row

### Fixed (v2026.02.02.05.40.892)

- **Non-ready drive handling** — Drives that are not ready (USB unplugged, optical drive empty, etc.) are now skipped gracefully with log notices instead of crashing the dialog
- **Select Drive auto-close** — Dialog now closes immediately when a drive is selected
- **Select Drive Cancel button layout** — Cancel button is now horizontally centered in the dialog
- **Button icon clipping** — Quick access button icons no longer clip at edges

### Technical Details (v2026.02.02.05.40.892)

- **Path editing**: Uses overlapping Grid container with TextBlock/TextBox visibility toggle
- **Drive selection**: Checks `DriveInfo.IsReady` before accessing drive properties
- **Dialog closure**: Drive buttons call `dialog.Hide()` immediately after path change
- **Theme matching**: Dialog gets `RequestedTheme` from parent window's `ActualTheme`
- **Button improvements**: Width=48, Height=42, FontIcon.FontSize=18, Padding=4

---

## [v2026.02.02.05.10.427] - 2026-02-02 - Initial Folder Picker UX Improvements

### Added (v2026.02.02.05.10.427)

- **Inline path editing** — Click directly on the path text in the folder picker to edit it in place. Press Enter to save or Escape to cancel.
- **Home button quick access** — Moved Home button from navigation bar to the top quick access row (left of Desktop) for easier access.

### Changed (v2026.02.02.05.10.427)

- **Folder picker navigation simplification** — Streamlined UI by integrating Home into quick access and making path directly clickable without requiring an Edit button.

---

## [v2026.02.02.04.52.634] - 2026-02-02 - Folder Picker Window Improvements

### Added (v2026.02.02.04.52.000)

- **Folder picker window persistence** — Save/restore position, size, and last path with center fallback

### Changed (v2026.02.02.04.52.000)

- **Main window backdrop** — Apply DesktopAcrylic frosted glass effect for parity with folder picker
- **Folder picker title bar** — Theme-matched title bar colors for light/dark mode
- **Folder picker UX** — Removed confusing “Selected” status text and removed refresh button

### Fixed (v2026.02.02.04.52.000)

- **Folder picker crash** — Removed blocking async pattern to prevent AggregateException on open

## [v2026.02.02.03.05.000] - 2026-02-02 - Motion System & Control Polish Complete

### Added (v2026.02.02.03.05.000)

- **Motion System** — Consistent hover/press animations aligned to WinUI 3 timing guidance
- **Keyboard Navigation** — Verified tab order with non-interactive elements removed from tab stop

### Changed (v2026.02.02.03.05.000)

- **MainPage.xaml** — Control polish
  - Consistent 40x40 control sizing and inline CornerRadius/Thickness usage
  - Shared pointer handlers for hover/press feedback
  - Explicit TabIndex ordering for primary controls

- **MainPage.xaml.cs** — Motion timings
  - Entrance animation timing adjusted to 167ms
  - Hover scale 1.02 and press scale 0.98 with 100ms durations

## [v2026.02.02.02.50.000] - 2026-02-02 - Console Log Scroll Perfected

### Changed (v2026.02.02.02.50.000)

- **MainPage.xaml.cs Log Output Rendering** — Restored full scrollback while eliminating partial lines
  - Renders all log entries again (no clipping of history)
  - Always auto-scrolls to the newest entry when new logs are added (even if user scrolled up)
  - Snaps scroll offsets to exact line-height increments to prevent partial line display
  - Size changes now preserve consistent scroll alignment

## [v2026.02.02.18.45.000] - 2026-02-02 - Theme-Aware Console & Line Clipping

### Added (v2026.02.02.18.45.000)

- **Console Output Line Clipping** — Prevents partial text display when terminal size cuts off lines
  - Algorithm: Calculates visible ScrollViewer height ÷ 20px line height = max complete lines
  - Only displays lines that fit completely within visible area (no mid-line text cutoff)
  - Shows most recent log entries that fit, maintains 100-line rolling buffer in memory
  - Improves console output appearance

### Changed (v2026.02.02.18.45.000)

- **MainPage.xaml.cs GetThemeAwareBrush()** — Critical root cause fix for light mode text color issue
  - **Root Cause Identified**: Method was calling `Application.Current.Resources.TryGetValue()` which doesn't reliably access WinUI 3 ResourceDictionary.ThemeDictionaries at runtime
  - **Solution**: Removed resource lookup entirely; now uses only `this.ActualTheme == ElementTheme.Light` property (checked at render time)
  - **Color Mapping**: Light theme = dark text RGB(26,26,26); Dark theme = bright text RGB(255,255,255)
  - **Result**: Light mode now correctly displays dark text on light background; dark mode displays bright text on dark background
  - All 7 color types properly theme-aware: LogTimestampBrush, LogInfoBrush, LogWarningBrush, LogErrorBrush, LogSuccessBrush, LogDebugBrush, default

- **MainPage.xaml.cs UpdateLogText()** — Implemented line clipping calculation
  - Added height calculation: `visible height / 20px line height = max complete lines`
  - Filters log entries before rendering to only show lines that fit completely
  - Prevents partial text display at terminal boundaries

### Fixed (v2026.02.02.18.45.000)

- **settings.json JSON Schema Validation Errors**
  - Removed conflicting extension settings that violated schema (chat.tools.terminal.autoApprove, etc.)
  - Cleaned up MCP server settings to prevent validation errors
  - Result: 0 schema validation errors; clean JSON structure

- **WORKFLOW.md Markdown Linting Violations**
  - MD032: Fixed lists not surrounded by blank lines (removed internal blank lines from single lists)
  - MD022: Added blank line before "## Code Organization" heading
  - Removed duplicate code lines in PowerShell examples
  - Result: All markdownlint violations resolved; file validates cleanly

### Status (v2026.02.02.18.45.000)

- ✅ **Core UI baseline validated**
  - All core UI features implemented and tested
  - Build verified clean (0 errors, 0 warnings)
  - Batch rename engine and advanced features remain in progress

## [v2026.02.01.22.27.000] - 2026-02-01 - Window State Persistence System

### Added (v2026.02.01.22.27.000)

- **WindowSettings.cs Helper Class** — Centralized persistent storage for window state (position, size, theme)
  - JSON-based settings file: `C:\Users\[User]\AppData\Local\Or1nRenameFileNameToDate\window-settings.json`
  - Smart path resolution: tries ApplicationData.LocalFolder → falls back to LocalAppData for debug mode
  - Methods: `LoadAsync()`, `SaveAsync(x, y, w, h)`, `SaveThemeAsync(theme)`, `IsValidPosition()`, `GetCenteredPosition()`
  - Debug logging to `or1n-window-debug.log` for troubleshooting persistence issues
  - Properties: X, Y, Width, Height, Theme (JSON schema)

- **Window Position Persistence** — Saves and restores window X/Y coordinates on app close/launch
  - Position validation against current display configuration prevents off-screen restoration
  - Multi-monitor aware via DisplayArea.GetFromWindowId()
  - Centers window on first launch if saved position is invalid or file doesn't exist

- **Window Size Persistence** — Saves and restores window width/height on app close/launch
  - Size restoration happens BEFORE position restoration (prevents cascading failures)
  - User-calibrated minimum size: 710x640px (ensures no UI cutoff at minimum window size)
  - Enforced via `EnforceMinimumWindowSize()` called on AppWindow.Changed event

- **Theme Persistence** — Saves and restores Light/Dark theme preference across sessions
  - Independent theme saving via `WindowSettings.SaveThemeAsync(theme)`
  - Loads saved theme on app startup; falls back to system theme if no preference saved
  - Persists theme separate from window state to prevent blocking on size changes

### Changed (v2026.02.01.22.27.000)

- **MainWindow.xaml.cs** — Complete window state management integration
  - Added `_minWidth = 710`, `_minHeight = 640` constants (user-tested minimum viable size)
  - Added `_isInitializing` flag to prevent erroneous saves during startup
  - Constructor: Calls `WindowSettings.ClearDebugLog()`, `RestoreOrCenterWindow()`, attaches event handlers
  - New method `RestoreOrCenterWindow()`: Async load of saved state with intelligent restoration order and validation
  - New method `SaveWindowPosition()`: Fire-and-forget async save during runtime (position/size changes)
  - New method `SaveWindowPositionSync()`: **Critical fix** — Blocking 200ms-timeout save on close event
  - New method `EnforceMinimumWindowSize()`: Resizes window if user attempts to go below minimum
  - New event handler `AppWindow_Changed_SavePosition()`: Auto-saves on any position/size change for continuous persistence
  - New event handler `SaveWindowPositionSync()`: Blocks close event until settings saved (200ms timeout prevents hang)

- **MainPage.xaml.cs** — Theme persistence integration
  - Modified `SetThemeComboToSystemPreference()`: Now loads saved theme preference via `WindowSettings.LoadAsync()` with blocking Task.Run()
  - Modified `ThemeComboBox_SelectionChanged()`: Added call to `WindowSettings.SaveThemeAsync(tag)` after theme change

- **Program.cs** — Removed hardcoded window sizing
  - Removed: `window.AppWindow.Resize(new SizeInt32 { Width = 900, Height = 700 })`
  - Reason: Was forcing all restores to 900x700, overriding `RestoreOrCenterWindow()` logic
  - Now: Lets `RestoreOrCenterWindow()` handle all sizing based on saved state or first-launch centering

- **Window Close Performance** — **Critical optimization**
  - Reduced `SaveWindowPositionSync()` timeout from 5 seconds → 200ms
  - Result: X-button close is instant; no apparent hang while settings save asynchronously
  - Verified: Settings still save correctly within 200ms window (measured ~20-50ms typical save time)

### Fixed (v2026.02.01.22.27.000)

- **Window Sizing Order** — Position restoration was overwriting size from saved state
  - Now: Restore size FIRST, then validate/restore position SECOND
  - Prevents position update from forcing window back to default size

- **Invalid Position Handling** — Restored position could place window off-screen on display config changes
  - Now: `IsValidPosition()` validates that window center point intersects with current DisplayArea
  - Falls back to centering on current display if position is invalid

- **Theme Loading Timing** — Theme was loading asynchronously but needed synchronously during init
  - Now: `SetThemeComboToSystemPreference()` blocks on `Task.Run()` to ensure theme loads before UI renders

- **Settings Save on Close** — Async fire-and-forget save wasn't completing before app exit
  - Now: `SaveWindowPositionSync()` blocks with explicit `.Wait(200ms)` timeout on Closed event
  - Graceful: If save exceeds 200ms, app still closes; settings will be correct on next launch

- **Minimum Size Calibration** — Minimum was too small (600x420), causing UI text/log cutoff
  - User manually resized to comfortable viewing size (719x652)
  - Extracted actual minimum from testing: 710x640px
  - Verified: All UI elements (title 2 lines, description, buttons, log area) fit without cutoff

### Technical Highlights (v2026.02.01.22.27.000)

- **Async/Await Pattern**: `SaveWindowPositionSync()` uses explicit `.Wait()` with timeout for blocking close event
- **Multi-Monitor Support**: Display validation uses `DisplayArea.GetFromWindowId()` → `DisplayArea.GetFromPoint()` fallback
- **Debug Logging**: Timestamp-prefixed logs in `or1n-window-debug.log` show complete lifecycle:
  - `LoadAsync()`: What was loaded from disk
  - `SaveAsync()`: Position/size saved with timestamp
  - `IsValidPosition()`: Validation results and display info
  - `RestoreOrCenterWindow()`: Step-by-step restoration process
- **Error Handling**: All I/O exceptions caught and logged; invalid JSON gracefully falls back to defaults
- **Performance**: Size enforcement via throttled AppWindow.Changed handler (~100ms delay to prevent rapid cascading)

### Status Update (v2026.02.01.22.27.000)

**Window persistence complete:**

- ✅ Window position save/restore with multi-monitor awareness
- ✅ Window size save/restore with intelligent minimum (710x640px)
- ✅ Theme preference persistence (Light/Dark)
- ✅ Instant app close (200ms timeout optimization)
- ✅ Debug logging for troubleshooting

**Foundation status:**

- ✅ Theme system (UIConfig.xaml with Light/Dark ThemeDictionaries)
- ✅ Responsive breakpoints (Compact/Medium/Wide VisualStates)
- ✅ Entrance animations (EntranceThemeTransition)
- ✅ Text scaling support (IsTextScaleFactorEnabled)
- ✅ Accessibility labels (AutomationProperties on controls)
- ⚠️ Mica Alt backdrop (code exists, needs user verification if working)
- ⚠️ Full design system implementation (spacing/control polish/animations ongoing)

**Next:** Continue WinUI 3 design items and core rename functionality work.

---

## [v2026.02.01.19.38.388] - 2026-02-01 - Documentation, Bootstrap, and Cleanup Updates

### Added (v2026.02.01.19.38.388)

- XML documentation for `Program`, `App`, and `WindowHelper` classes and members

### Changed (v2026.02.01.19.38.388)

- Switched to Windows App SDK `Bootstrap.TryInitialize` usage in Program.cs
- Fixed clean_winui.ps1 string formatting for reliable PowerShell execution

### Removed (v2026.02.01.19.38.388)

- Session report and pre-commit checklist documents after completion

## [v2026.02.01.19.05.123] - 2026-02-01 - Build System and Launch Critical Fixes

### Fixed (v2026.02.01.19.05.123)

- App Launch Crash (0xc000027b): Fixed critical exception in Microsoft.UI.Xaml.dll preventing app launch
  - Changed App.xaml from `<Page>` to `<ApplicationDefinition>` in .csproj (required for WinUI 3)
  - Disabled auto-includes with `EnableDefaultCompileItems=false` and `EnableDefaultPageItems=false`
  - Added explicit file declarations with Link attributes to maintain src/ structure while flattening build output
- Window Creation Pattern: Moved window instantiation from App.OnLaunched to Program.cs following WinUI 3 .exe launch pattern
- XAML Compilation: Resolved XAML files being compiled to wrong location (src/App.xbf → App.xbf) with Link attributes
- NETSDK1022 Error: Fixed duplicate items error from conflicting auto-includes and manual declarations

### Added (v2026.02.01.19.05.123)

- .gitignore: Created comprehensive Git ignore file excluding build artifacts (`bin/`, `obj/`), debug files (`debug*.txt`, `*.log`), temporary files (`_temp.txt`), and WinUI build outputs (`*.xbf`, `*.pri`)

### Removed (v2026.02.01.19.05.123)

- Temporary Files: Cleaned debug_log.txt, debug2.txt, and docs/_temp.txt from repository

### Technical Notes (v2026.02.01.19.05.123)

- Critical fix ensures WinUI 3 works correctly with src/ folder organization
- Deep clean (clean_winui.ps1) required after .csproj changes to clear stale XAML compiler artifacts
- App now launches successfully: verified Process ID, MainWindowHandle, and window responds correctly
- Build succeeds with 0 errors, 0 warnings

## [v2026.02.01.18.29.25.030] - 2026-02-01 - Foundation Release: Core UI and Theme System + Project Reorganization

### Added (v2026.02.01.18.29.25.030)

- Complete Theme System: Implemented UIConfig.xaml with ThemeDictionaries, 5 light and 5 dark colors per theme, automatic theme-aware styling for all UI elements
- Title Bar Theming: Custom title bar colors matching light/dark/system themes with properly styled minimize/maximize/close buttons and instant theme switching
- WindowHelper Utility: Helper class for window tracking and communication between MainPage and MainWindow for theme propagation
- Centralized Resource Management: All sizing constants (padding, margins, font sizes, corner radius, spacing) moved to UIConfig.xaml
- Build Utilities: Added clean_winui.ps1 deep clean script for resolving WinUI 3 build issues
- Editor Configuration: Added .editorconfig for consistent coding style across editors and XAML IntelliSense error suppression
- OmniSharp Config: Added omnisharp.json to suppress false-positive CS0103/CS1061 errors for XAML-generated code
- MIT License: Added LICENSE file to project root

### Changed (v2026.02.01.18.29.25.030)

- Project Structure: Reorganized all source files into `src/` folder following industry best practices (src/App.xaml, src/MainWindow.xaml, src/Views/, src/UIConfig.xaml)
- Documentation Organization: Moved all markdown documentation to `docs/` folder for better discoverability (docs/TODO.md, docs/FILES_AND_FOLDERS.md, docs/SETUP.md, docs/WORKFLOW.md, docs/CHANGELOG.md, docs/README.md)
- Window Configuration: Fixed window size to 900x700 and set proper window title
- UI Element Text: Simplified "Open Folder" to "Open", updated theme ComboBox labels to "System theme", "Light theme", "Dark theme"
- Layout and Presentation: Removed folder path display next to buttons, fully centered subtitle text, changed initial terminal text to action prompt with accent color

### Fixed (v2026.02.01.18.29.25.030)

- Debugger Issue: Changed launch.json to launch .exe instead of .dll for proper WinUI 3 runtime initialization (F5 debugging now works)
- XAML Resource Resolution: Resolved XamlParseException errors by replacing missing resource references with proper theme-aware brushes
- Theme Switching: Fixed title bar not updating when switching themes, fixed button colors in dark mode, improved theme propagation across UI
- Layout Issues: Fixed subtitle text alignment and folder selection tracking
- Documentation Quality: Fixed 36 markdown linting violations across all documentation files
  - MD009 (trailing spaces), MD001 (heading level increments), MD037 (emphasis marker spacing)
  - MD031/MD040 (code fence blank lines and language tags), MD022 (heading blank lines)
  - MD060 (table column spacing), MD012 (multiple blank lines), MD032 (list blank lines)
  - MD024 (duplicate headings), MD051 (invalid link fragments)
- Documentation Accuracy: Updated FILES_AND_FOLDERS.md to reflect new src/ and docs/ structure with complete file descriptions

## [2026-01-31]

- Window sizing and adaptive layout improvements using WinUI 3 AppWindow API
- Log area layout optimization with grid row sizing
- Code cleanup and best-practice layout implementation

## [Initial]

- Project scaffolded with WinUI 3, placeholder UI, README and TODO created, initial requirements and install instructions
