# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project uses timestamp-based versioning in the format `v{YYYY}.{MM}.{DD}.{HH}.{mm}.{MSS}`.

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
