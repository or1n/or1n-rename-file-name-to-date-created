# Project Structure & File Guide

This is a short, practical map of the repo so new contributors can find things quickly.

---

## 📁 Top-Level Layout

```text
or1n_rename_file-names-to-date/
├── src/                # All application code
├── docs/               # Documentation (concise, no duplication)
├── Assets/             # App icons and branding
├── .github/            # Repo-level guidance
├── .vscode/            # VS Code tasks/settings
├── app.manifest        # Windows app manifest
├── Package.appxmanifest# MSIX manifest (if packaging is used)
├── or1n-rename-file-name-to-date-created.csproj
├── or1n-rename-file-name-to-date-created.slnx
├── install.cmd         # Setup script
├── clean_winui.ps1     # Deep clean script
└── .gitignore
```

---

## 📦 Source Code (src/)

```text
src/
├── App.xaml / App.xaml.cs         # App resources + lifecycle
├── MainWindow.xaml / MainWindow.xaml.cs
│   └── Main window, title bar, backdrop, persistence
├── UIConfig.xaml                  # Theme resources and sizing
├── Program.cs                     # App entry point
├── Imports.cs                     # Global usings
├── Helpers/
│   ├── WindowHelper.cs            # Window tracking utilities
│   ├── WindowSettings.cs          # Main window persistence
│   └── FolderBrowserSettings.cs   # Folder picker persistence
└── Views/
    ├── MainPage.xaml / MainPage.xaml.cs
    └── FolderBrowserDialog.xaml.cs
```

---

## 📂 Documentation (docs/)

- README.md — documentation index (links to the key docs)
- SETUP.md — install/run instructions
- WORKFLOW.md — build/run/debug tips
- TODO.md — current work list
- CHANGELOG.md — version history
- WINUI3_DESIGN_GUIDE.md — UI design rules

---

## ⚙️ Config & Tooling

- or1n-rename-file-name-to-date-created.csproj — project settings, package refs
- .vscode/launch.json — debug configs (launches .exe)
- .vscode/tasks.json — build/clean tasks
- .editorconfig / omnisharp.json — editor rules and diagnostics

---

## 🔧 Build Artifacts (ignored by git)

- bin/, obj/, artifacts/ — generated during build

---

## 📌 Quick Reference

| Need | Location |
| --- | --- |
| Theme colors/sizing | src/UIConfig.xaml |
| Main UI layout | src/Views/MainPage.xaml |
| Main window logic | src/MainWindow.xaml.cs |
| Folder picker | src/Views/FolderBrowserDialog.xaml.cs |
| Docs overview | docs/README.md |
