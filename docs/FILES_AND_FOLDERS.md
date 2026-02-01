# Project Structure & File Guide

This document describes the complete professional enterprise structure of or1n, what each file does, and where to find things.

---

## 📁 Directory Structure Overview

### Professional Enterprise Layout

```text
or1n_rename_file-names-to-date/
│
├── 📂 src/                          ⭐ SOURCE CODE (all your code goes here)
│   ├── Views/                       # UI pages and screens
│   ├── Helpers/                     # Utility classes
│   ├── App.xaml & App.xaml.cs
│   ├── MainWindow.xaml & MainWindow.xaml.cs
│   ├── UIConfig.xaml                # Central theme/design system
│   ├── Program.cs
│   └── Imports.cs
│
├── 📂 docs/                         ⭐ DOCUMENTATION (all guides go here)
│   ├── README.md
│   ├── SETUP.md
│   ├── WORKFLOW.md
│   ├── TODO.md
│   ├── CHANGELOG.md
│   ├── FILES_AND_FOLDERS.md
│   └── WINUI3_DESIGN_GUIDE.md    # WinUI 3 design patterns & best practices
│
├── 📂 .github/                      GitHub configuration
├── 📂 .vscode/                      VS Code settings & tasks
├── 📂 Assets/                       App icons and images
├── 📂 bin/                          Build output (auto-generated, ignored in git)
├── 📂 obj/                          Intermediate build files (auto-generated, ignored in git)
├── 📂 artifacts/                    Build artifacts (auto-generated, ignore in git)
│
├── ⚙️  Configuration Files
├── 📚 Documentation Files
└── 🚀 Build/Launch Scripts

```

---

## 📦 Source Code Structure (`src/`)

### Core Application Files (in `src/`)

#### `Program.cs`

- **Purpose**: Bootstrap entry point for the application

- **Location**: `src/Program.cs`
- **What it does**:
  - Initializes WinRT COM wrappers
  - Bootstraps Windows App SDK
  - Creates DispatcherQueue for thread management
  - Launches the App

- **When to modify**: Very rarely - only for startup changes
- **Key methods**: `Main()`

- **Related**: `src/App.xaml.cs` (app lifecycle continues)

#### `App.xaml` & `App.xaml.cs`

- **Purpose**: Application-level configuration and resource management

- **What it does**:
  - Defines app-level resources (colors, styles, strings)
  - Merges UIConfig.xaml resources for theming
  - Handles application lifecycle (OnLaunched)
  - Creates the main window

- **When to modify**: When adding app-wide resources or handling app events
- **Key methods**: `OnLaunched()`, constructor

- **Related**: `src/UIConfig.xaml` (merged resources), `src/MainWindow.xaml.cs`

#### `MainWindow.xaml` & `MainWindow.xaml.cs`

- **Purpose**: Top-level window container and frame navigation

- **What it does**:
  - Contains the navigation Frame (RootFrame)
  - Manages the window itself (size, title, properties)
  - Sets up custom title bar with theme-aware colors
  - Listens for theme changes and updates title bar accordingly
  - Tracks active windows for inter-window communication

- **When to modify**: When changing window behavior, title bar, or navigation logic
- **Key methods**: `SetupTitleBar()`, `UpdateTitleBarTheme()`, `RootFrame_ActualThemeChanged()`

- **Related**: `src/App.xaml.cs`, `src/Helpers/WindowHelper.cs`, `src/Views/MainPage.xaml.cs`

#### `UIConfig.xaml`

- **Purpose**: Centralized theme and design system

- **What it does**:
  - Defines Light theme colors (5 background colors, 5 text colors)
  - Defines Dark theme colors (5 background colors, 5 text colors)
  - Creates brushes for each color
  - Defines named sizes (padding, margins, font sizes, spacing)
  - All UI uses these resources (no hardcoded colors)

- **When to modify**: When adjusting colors, spacing, or font sizes
- **Key sections**:
  - `ResourceDictionary.ThemeDictionaries["Light"]` - Light theme colors
  - `ResourceDictionary.ThemeDictionaries["Dark"]` - Dark theme colors
  - Shared sizing resources at bottom

- **Related**: All XAML files reference these resources
- **This is your design system!** Edit here for any styling changes.

#### `Imports.cs`

- **Purpose**: Global using statements for common namespaces

- **What it does**:
  - Reduces boilerplate in all C# files
  - Imports WinUI, .NET, and Windows API namespaces
  - Documents future library needs (metadata extraction)

- **When to modify**: When adding significant new features requiring new global namespaces
- **Key imports**: `Microsoft.UI.Xaml`, `Windows.Storage`, `System.IO`

- **Future**: Will include references to metadata extraction libraries (Phase 5+)

---

### `src/Helpers/` Folder

#### `Helpers/WindowHelper.cs`

- **Purpose**: Utility for window tracking and communication

- **Location**: `src/Helpers/WindowHelper.cs`
- **What it does**:
  - Maintains a list of active windows
  - Provides method to find a window by its XAML element
  - Enables inter-component communication (e.g., Views → MainWindow)

- **Key methods**: `GetWindowForElement()` - finds window for a UI element
- **Usage**: Used by MainPage to update MainWindow's title bar when themes change

#### `Helpers/WindowSettings.cs`

- **Purpose**: Persistent storage for window state (position, size, theme)

- **Location**: `src/Helpers/WindowSettings.cs`
- **What it does**:
  - Manages JSON-based settings file in `LocalAppData\Or1nRenameFileNameToDate\window-settings.json`
  - Smart path resolution: tries ApplicationData.LocalFolder, falls back to LocalAppData
  - Saves/loads window position (X, Y), size (Width, Height), and theme preference
  - Validates saved position against current display configuration (multi-monitor safe)
  - Provides debug logging to `or1n-window-debug.log` for troubleshooting

- **Key methods**:
  - `LoadAsync()` - Loads settings from JSON file; returns cached result on subsequent calls
  - `SaveAsync(x, y, w, h)` - Saves position/size while preserving theme
  - `SaveThemeAsync(theme)` - Saves theme independently
  - `IsValidPosition(settings, size)` - Validates position against current display
  - `GetCenteredPosition(displayArea)` - Centers window on specified display

- **JSON Schema**:
  ```json
  {
    "X": number,
    "Y": number,
    "Width": number,
    "Height": number,
    "Theme": "Light" | "Dark"
  }
  ```

- **Usage**: Called by MainWindow during startup and on resize/move, called by MainPage on theme change
- **Namespace**: `Or1nRenameFileNameToDateCreated.Helpers`

---

### `src/Views/` Folder

#### `Views/MainPage.xaml` & `Views/MainPage.xaml.cs`

- **Purpose**: Main UI page with controls for folder selection and file scanning

- **Location**: `src/Views/MainPage.xaml` and `src/Views/MainPage.xaml.cs`
- **What it does**:
  - 4-row grid layout: Title → Subtitle → Theme Selector → Buttons → Log Area
  - Implements folder picker functionality
  - Implements file scanner (currently demo/placeholder)
  - Manages real-time logging with 100-line rolling buffer
  - Handles theme switching and persists preference via WindowSettings
  - Responsive design with 3 visual states (Compact/Medium/Wide)

- **When to modify**: When adding new UI controls or changing layout
- **Key methods**:
  - `ThemeComboBox_SelectionChanged()` - handles theme changes and calls `WindowSettings.SaveThemeAsync()`
  - `SetThemeComboToSystemPreference()` - loads saved theme preference on startup
  - `OpenFolderButton_Click()` - folder picker dialog
  - `ScanButton_Click()` - file scanner placeholder
  - `Log()` - real-time logging system

- **Related**: `src/UIConfig.xaml` (theme resources), `src/MainWindow.xaml.cs` (title bar updates), `src/Helpers/WindowSettings.cs` (theme persistence)
- **Namespace**: `Or1nRenameFileNameToDateCreated.Views`

---

## 📂 Documentation Structure (`docs/`)

All documentation files are organized in the `docs/` folder for easy discovery and organization.

### `docs/README.md`

- **Purpose**: Project overview and getting started guide

- **What it contains**:
  - Project title and description
  - Status (v1.0 Shell - early development)
  - Current features (UI, theming - no core functionality yet)
  - Placeholder/unimplemented features (batch rename, metadata extraction)
  - System requirements (Windows 11, .NET 8.0)
  - Installation instructions (VS Code, Visual Studio, CLI)
  - Usage guide
  - Versioning scheme

- **Audience**: End users, if published on GitHub

#### `docs/SETUP.md`

- **Purpose**: Detailed setup and installation guide

- **What it contains**:
  - System requirements (OS, storage, processor)
  - IDE/editor options (VS Code vs Visual Studio 2022)
  - Step-by-step installation (automated, manual, IDE)
  - Verification steps
  - Dependency table
  - Troubleshooting guide
  - Next steps

- **Audience**: Developers setting up the project

#### `docs/WORKFLOW.md`

- **Purpose**: Development workflow and practical tips

- **What it contains**:
  - Quick build & run commands
  - Common development tasks (debugging, XAML changes, adding views)
  - Theme switching testing
  - Code organization reference
  - Performance notes
  - Contributing guidelines
  - References to official documentation

- **Audience**: Developers working on the codebase

#### `docs/TODO.md`

- **Purpose**: Development roadmap and progress tracking

- **What it contains**:
  - Phase 1 (v1.0) - ✅ Complete - Foundation & Core UI
  - Phase 2 (v1.1) - 🔄 In Progress - Enhanced Functionality
  - Phase 3 (v2.0) - 📋 Planned - Advanced UI & Controls
  - Phase 4 (v2.1) - 📋 Planned - Accessibility
  - Phase 5 (v3.0) - 📋 Planned - Core Feature (Batch Rename Engine)
  - Phase 6 (v3.1+) - 🎨 Future - Advanced Features
  - Summary by phase with percentages
  - Versioning scheme documentation

- **Audience**: Project managers, developers, stakeholders

#### `docs/CHANGELOG.md`

- **Purpose**: Version history and release notes

- **What it contains**:
  - Current version timestamp
  - Added features (theme system, title bar theming, etc.)
  - Changed items (window sizing, UI text, layout)
  - Fixed bugs (debugger issue, XAML resources, theme switching)
  - Historical entries for previous versions

- **Audience**: End users, developers, GitHub releases

#### `docs/FILES_AND_FOLDERS.md` (this file)

- **Purpose**: Complete project structure documentation
- **What it contains**: Descriptions of all files and folders, their purpose, and when to modify them

- **Audience**: Developers learning the codebase

---

## ⚙️  Configuration Files (Root Level)

### Build Configuration

#### `.csproj` (or1n-rename-file-name-to-date-created.csproj)

- **Location**: Root directory

- **Purpose**: Project configuration and NuGet package management
- **What it does**:
  - Defines target framework: `net8.0-windows10.0.19041.0`
  - Specifies NuGet package dependencies
  - Configures Windows App SDK (1.8.260101001)
  - Enables WinUI 3 build tools
  - Sets compiler options (nullable, latest C#)
  - Defines runtime identifiers (x86, x64, ARM64)

- **Key properties**:
  - `TargetFramework`: .NET 8.0 for Windows 10 Build 19041+
  - `WindowsAppSDK`: Version 1.8.260101001
  - `DisableXamlGeneratedMain`: Custom bootstrap (not auto-generated)

- **When to modify**: When updating SDK versions or adding new NuGet packages
- **Do NOT modify**: Build targets, properties, or paths unless you know what you're doing

#### `or1n-rename-file-name-to-date-created.slnx`

- **Location**: Root directory

- **Purpose**: Visual Studio solution file
- **When to modify**: Rarely - only when adding significant new projects

### VS Code Configuration (`.vscode/` folder)

#### `.vscode/launch.json`

- **Purpose**: Debug configuration for VS Code

- **What it does**:
  - Defines debug launch configurations
  - **CRITICAL**: Launches `.exe` (not `.dll`) - required for WinUI 3
  - Includes pre-launch build task
  - Provides multiple debug configurations (standard, x64, break on all exceptions, attach)

- **Key configurations**:
  - `.NET Core Launch (WinUI 3, Debug)` - default, launches exe with build
  - `.NET Core Launch (x64, native debug)` - native debugging
  - `.NET Core Attach` - attach to running process

- **When to modify**: Only if adding new debug scenarios
- **⚠️ CRITICAL**: Do NOT change to launch `.dll` - it breaks WinUI 3

#### `.vscode/settings.json`

- **Purpose**: VS Code workspace-specific settings

- **What it does**:
  - Auto-approves trusted build commands (dotnet, git, msbuild)
  - Sets C# formatter
  - Suppresses Roslyn analyzer notifications
  - Excludes false positive diagnostics from problems panel
  - Configures editor behavior

- **When to modify**: When changing development environment preferences

#### `.vscode/tasks.json`

- **Purpose**: Build and cleanup tasks for VS Code

- **What it does**:
  - Defines "Deep Clean WinUI" task - runs clean_winui.ps1
  - Defines "build" task - dotnet build with clean as dependency
  - Defines "publish" task - dotnet publish with clean as dependency
  - Defines "watch" task - dotnet watch run (auto-rebuild)

- **Usage**: Run with Ctrl+Shift+B in VS Code
- **When to modify**: When adding new build tasks or changing build process

### Code Style Configuration

#### `.editorconfig`

- **Location**: Root directory

- **Purpose**: Code style and formatting rules for the entire project
- **What it does**:
  - UTF-8 charset enforcement
  - Formatting rules for C# files (indentation, spacing)
  - Formatting rules for XAML files
  - Diagnostic suppression for known XAML false positives (CS0103, CS1061)

- **When to modify**: When adding new coding standards or changing formatting rules

#### `omnisharp.json`

- **Location**: Root directory

- **Purpose**: Configuration for the Roslyn code analyzer (OmniSharp)
- **What it does**:
  - Enables project loading on-demand
  - Configures Roslyn analyzers
  - Suppresses false positive warnings (CS0103, CS1061) for XAML code-behind
  - Enables custom code completion features

- **When to modify**: When adding new diagnostic suppressions or analysis rules

### Windows Application Manifests

#### `app.manifest`

- **Location**: Root directory

- **Purpose**: Application manifest for Windows App
- **What it does**:
  - Declares app identity
  - Specifies Windows version compatibility
  - Configures DPI awareness
  - Sets execution level (user, admin, etc.)

- **When to modify**: Rarely - only for Windows API level changes

#### `Package.appxmanifest`

- **Location**: Root directory

- **Purpose**: Windows package manifest for MSIX packaging
- **What it does**:
  - Originally generated, now may be edited for package configuration
  - References app capabilities, icons, splash screen

- **When to modify**: When creating MSIX package (Phase 4+)

---

## 🚀 Build & Installation Scripts (Root Level)

### `install.cmd`

- **Location**: Root directory

- **Purpose**: Automated installation and setup script (Windows command prompt)
- **What it does**:
  - Verifies Windows 11 requirement
  - Checks .NET SDK installation and version
  - Performs deep clean of previous builds (optional)
  - Restores NuGet packages
  - Builds the project
  - Provides next steps and troubleshooting

- **When to use**: First-time setup or after major build issues
- **Usage**: Run from command prompt: `install.cmd`

#### `clean_winui.ps1`

- **Location**: Root directory

- **Purpose**: PowerShell script for comprehensive cleanup
- **What it does**:
  - Terminates blocking processes (MSBuild, dotnet, app)
  - Removes build artifacts (bin/, obj/)
  - Clears IDE cache (.vs/, .vscode-test/)
  - Clears NuGet temp cache
  - Removes temporary files (`*.tmp`, `*.log`, `*.user`, etc.)

- **When to use**: Before building after major changes, or when build is stuck
- **Usage**: Run from PowerShell: `.\clean_winui.ps1`

- **Also**: Runs automatically before build task in VS Code

---

## 📁 GitHub Configuration (`.github/` folder)

### `.github/copilot-instructions.md`

- **Location**: `.github/copilot-instructions.md`

- **Purpose**: Custom instructions for GitHub Copilot AI assistant
- **What it contains**:
  - Project-specific guidance for AI-assisted development
  - Code style guidelines
  - Architecture patterns
  - Development best practices
  - Naming conventions
  - WinUI 3 patterns and tips
  - Known limitations and workarounds

- **When to modify**: When establishing new development standards

---

## 📁 Assets Folder

### `Assets/`

- **Location**: Root directory

- **Purpose**: Application icons, splash screens, and branding
- **Contents**:
  - `SplashScreen.scale-200.png` - Launch splash screen
  - `Square150x150Logo.scale-200.png` - App tile (medium)
  - `Square44x44Logo.scale-200.png` - App icon (small)
  - `StoreLogo.png` - Microsoft Store icon
  - `Wide310x150Logo.scale-200.png` - App tile (wide)
  - `LockScreenLogo.scale-200.png` - Lock screen logo

- **When to modify**: When updating branding or icons
- **Format**: PNG images at specified scales (2x for high-DPI displays)

---

## 📁 Build Artifacts (Auto-Generated - Ignore in Git)

### `bin/` Directory

- **Location**: `bin/` at root level
- **Purpose**: Compiled executable and runtime files

- **Contents**:
  - `Debug/net8.0-windows10.0.19041.0/` - Debug build output
    - `or1n-rename-file-name-to-date-created.exe` - ⭐ The running executable
    - `*.dll` - Compiled assemblies
    - `*.pdb` - Debug symbols
    - `Assets/` - App icon assets
  - `Release/` - Release build output (if published)

- **Auto-generated**: Yes, by `dotnet build`
- **Safe to delete**: Yes - will be regenerated on next build

- **Ignore in git**: Yes (in `.gitignore`)

### `obj/` Directory

- **Location**: `obj/` at root level
- **Purpose**: Intermediate build files and compiler output

- **Contents**:
  - `Debug/net8.0-windows10.0.19041.0/` - Intermediate compilations
    - `*.g.cs` - ⭐ XAML code-generated files (auto-generated code from XAML)
    - `*.g.i.cs` - XAML IntelliSense generated files
    - `.csproj.nuget.*` - Dependency resolution files
    - `priconfig.xml` - Resource configuration
  - Generated metadata and compiler cache

- **Auto-generated**: Yes, during build
- **Safe to delete**: Yes - will be regenerated on next build

- **Ignore in git**: Yes (in `.gitignore`)

### `artifacts/` Directory

- **Location**: `artifacts/` at root level
- **Purpose**: Build artifacts and logs

- **Contents**: Generated based on build process
- **Auto-generated**: Yes, during compilation

- **Safe to delete**: Yes - will be regenerated on next build
- **Ignore in git**: Yes (in `.gitignore`)

---

## 📝 Other Important Files

### `LICENSE`

- **Location**: Root directory
- **Purpose**: MIT License for the project

#### `.gitignore`

- **Location**: Root directory
- **Purpose**: Tells Git which files/folders NOT to track

#### `.git/` (hidden folder)

- **Location**: Root directory
- **Purpose**: Git version control repository

---

## 🔧 Special Tips

### About `.csproj` File Includes

This project disables default file includes and manually declares `.cs` and `.xaml` items to preserve the `src/` structure in build output.

### Namespaces Match Folders

```csharp
// src/Views/MainPage.xaml.cs
namespace Or1nRenameFileNameToDateCreated.Views { }

// src/Helpers/Helper.cs
namespace Or1nRenameFileNameToDateCreated.Helpers { }

```

---

## 📊 Quick Reference

| Need | Location |
| --- | --- |
| 🎨 Colors/theme | `src/UIConfig.xaml` |
| 📐 Layout | `src/Views/MainPage.xaml` |
| 🎯 Events | `src/Views/MainPage.xaml.cs` |
| 🪟 Window | `src/MainWindow.xaml.cs` |
| 📚 Docs | `docs/` folder |
| 🛠️ Build | `clean_winui.ps1` |

---

**Last Updated**: 2026-02-01 | **Version**: v1.0 | **Structure**: 2.0 Professional
