
# or1n-rename-file-name-to-date-created - Project Roadmap

## Overview

This document tracks the development progress of or1n, a modern WinUI 3 desktop application for batch renaming files based on their creation date. Items are organized by priority with completion percentages for incomplete tasks and timestamps for completed ones.

---

## 🔄 Active Tasks (Prioritized by Importance)

### Core Rename Functionality

- [ ] **Batch rename engine** — Parse filename, extension, and date components. (0%)

### Core Rename Functionality

- [ ] **Batch rename engine** — Parse filename, extension, and date components. (0%)
- [ ] **Apply formatting rules** — Date calculations and formatting logic. (0%)
- [ ] **Preview renamed filenames** — Show preview before applying changes. (0%)
- [ ] **Atomic batch rename** — All-or-nothing rename operation. (0%)
- [ ] **Handle name collisions** — Conflict resolution logic. (0%)

### Enhanced UI & Controls

- [ ] **Date/time format selector** — ComboBox for format selection. (0%)
- [ ] **Prefix/suffix input fields** — TextBox controls for custom text. (0%)
- [ ] **Custom separator options** — Separator character selection. (0%)
- [ ] **Numbering scheme selector** — Support for 001, 1, i, I formats. (0%)
- [ ] **File type filter combobox** — Filter by `*.jpg`, `*.mp4`, all files. (0%)
- [ ] **Date range picker** — Filtering by date range. (0%)
- [ ] **Size range slider** — Filtering by file size. (0%)
- [ ] **ProgressRing** — Long-running operations feedback. (0%)
- [ ] **ContentDialog** — Confirmation dialogs ("Apply 247 renames?"). (0%)
- [ ] **InfoBar** — Operation results and warnings display. (0%)
- [ ] **MenuBar** — File/Edit/View menu structure. (0%)
- [ ] **RichEditBox** — Preview and details view. (0%)

### Animations & Visual Effects

- [ ] **Smooth theme switching animations** — Animated theme transitions. (0%)
- [ ] **Control entry/exit animations** — Element appearance animations. (0%)
- [ ] **Button press feedback animations** — Visual feedback on interaction. (0%)
- [ ] **Acrylic blur effects** — Layered blur effects. (0%)
- [ ] **Theme-aware shadows and depth** — Modern shadow system. (0%)
- [ ] **Card-based layouts** — Sectioned card design. (0%)
- [ ] **Icon support** — FontIcon with theme variants. (0%)
- [ ] **Custom brushes and gradients** — Enhanced color palette. (0%)

### Accessibility & Compliance

- [ ] **Screen reader support** — Full Narrator compatibility. (0%)
- [ ] **Tab order optimization** — Logical keyboard navigation flow. (0%)
- [ ] **Keyboard shortcuts** — Common action shortcuts. (0%)
- [ ] **Focus indicators in all themes** — Visible focus outlines. (0%)
- [ ] **WCAG AAA color contrast** — High contrast testing and compliance. (0%)
- [ ] **High contrast theme** — HC mode in UIConfig.xaml. (0%)

### Advanced Features

- [ ] **Save/load rename profiles** — Reusable rename configurations. (0%)
- [ ] **Built-in presets** — Standard Date, ISO Date templates. (0%)
- [ ] **Profile import/export** — Share configurations. (0%)
- [ ] **Undo/redo support** — Transaction history management. (0%)
- [ ] **Bulk undo** — Revert rename operations. (0%)
- [ ] **Settings/preferences page** — Application configuration UI. (0%)
- [ ] **Regular expressions** — RegEx pattern support. (0%)
- [ ] **Complex date queries** — Advanced date range filtering. (0%)
- [ ] **Custom scripting** — User script support. (0%)

---

## ✅ Completed Tasks

### Folder Picker Window (Completed)

- [x] **Custom WinUI 3 folder picker window** — Replaced native picker with custom Window-based dialog. (2026-02-02 04:52)
- [x] **Folder picker title bar theming** — Matches light/dark title bar colors from main window. (2026-02-02 04:52)
- [x] **Folder picker state persistence** — Position, size, and last path save/restore with center fallback. (2026-02-02 04:52)
- [x] **Folder picker UX cleanup** — Removed confusing “Selected” status text and refresh button. (2026-02-02 04:52)
- [x] **Main window acrylic parity** — DesktopAcrylic applied to main window for consistent frosted glass effect. (2026-02-02 04:52)

### WinUI 3 Design & Polish (Completed)

- [x] **Log text selection** — Click to select text, CTRL+A select all, CTRL+C copy to clipboard. Native TextBox selection and copying fully functional. (2026-02-02 00:51)
- [x] **Log context menu** — Right-click menu with Copy Selected, Copy All, and Clear Log options. (2026-02-02 02:50)
- [x] **Log scrolling & line snapping** — Full scrollback with line-height snapping to prevent partial lines and auto-jump to newest entry on new logs. (2026-02-02 02:50)
- [x] **Motion system** — Consistent durations/easing for hover/press/enter/exit. (2026-02-02 03:05)
- [x] **Keyboard navigation** — Tab order verified; non-interactive items not tab-stoppable. (2026-02-02 03:05)
- [x] **Inline CornerRadius/Thickness** — Use inline values only (WinUI 3 XAML compiler limitation). (2026-02-02 03:05)
- [x] **Control polish** — Consistent button sizes, spacing, and corner radius. (2026-02-02 03:05)
- [x] **Mica Alt + fallback** — MicaController with BaseAlt, DesktopAcrylicBackdrop fallback. (2026-02-01 23:55)
- [x] **Theme-aware resource system** — All colors via ThemeDictionaries in UIConfig.xaml. (2026-02-01 19:38)
- [x] **Spacing system** — 4px base spacing scale defined. (2026-02-01 19:38)
- [x] **Responsive breakpoints** — Compact/Medium/Wide VisualStates with AdaptiveTrigger. (2026-02-01 19:38)
- [x] **Entrance animations** — Page transitions with EntranceThemeTransition. (2026-02-01 19:38)
- [x] **Focus indicators** — UseSystemFocusVisuals on all interactive controls. (2026-02-01 19:38)
- [x] **Text scaling** — IsTextScaleFactorEnabled on all text elements. (2026-02-01 19:38)
- [x] **Accessibility labels** — AutomationProperties on all controls. (2026-02-01 19:38)
- [x] **Resource dictionary hygiene** — Centralized naming conventions. (2026-02-01 19:38)

### Folder Picker UX Enhancements (Completed)

- [x] **Inline path editing (same location)** — Click path text to edit in place; TextBox replaces TextBlock in same grid cell. (2026-02-02 05:40)
- [x] **Drives/This PC button** — Quick access button to select from all available drives via dialog. (2026-02-02 05:40)
- [x] **Hover feedback on path text** — Opacity change and tooltip indicate path is clickable. (2026-02-02 05:40)
- [x] **Fixed quick access button icons** — Increased button size (48x42) and icon size (18px) for proper display. (2026-02-02 05:40)
- [x] **Select Drive dialog theme support** — Dialog respects light/dark theme matching folder picker window. (2026-02-02 05:40)
- [x] **Select Drive dialog auto-close** — Dialog closes immediately when a drive is selected. (2026-02-02 05:40)
- [x] **Select Drive centered Cancel button** — Cancel button positioned at center with proper spacing. (2026-02-02 05:40)
- [x] **Handle non-ready drives gracefully** — Skips unmounted/non-ready drives and logs notices instead of crashing. (2026-02-02 05:40)

### Window State Management

- [x] **Window Position Persistence** — Save/restore X,Y coordinates. (2026-02-01 17:15)
- [x] **Window Size Persistence** — Save/restore width/height. (2026-02-01 17:15)
- [x] **Theme Persistence** — Save/restore Light/Dark preference. (2026-02-01 17:15)
- [x] **Intelligent Minimum Size** — 710x640px prevents UI cutoff. (2026-02-01 17:15)
- [x] **Multi-monitor Support** — Position validation prevents off-screen. (2026-02-01 17:15)
- [x] **Instant Close Performance** — 200ms timeout, X-button closes immediately. (2026-02-01 17:15)
- [x] **Debug Logging** — `or1n-window-debug.log` for troubleshooting. (2026-02-01 17:15)
- [x] **Error Handling** — Graceful fallback for corrupted settings. (2026-02-01 17:15)
- [x] **JSON Settings Schema** — Compact storage (X, Y, Width, Height, Theme). (2026-02-01 17:15)

### Foundation & Core UI

- [x] **Main window setup** — Fixed size, custom title bar, window lifecycle. (2026-01-31 12:00)
- [x] **Page layout with responsive grid** — 5-row grid layout. (2026-01-31 12:00)
- [x] **Folder picker integration** — Native Windows folder browser. (2026-01-31 12:00)
- [x] **File scanner** — Directory enumeration, file grouping by extension. (2026-01-31 12:00)
- [x] **Real-time logging system** — 100-line rolling buffer, timestamped entries. (2026-01-31 12:00)
- [x] **UIConfig.xaml centralized resources** — ThemeDictionaries for Light/Dark. (2026-01-31 12:00)
- [x] **Dynamic theme switching** — ComboBox selection, instant application. (2026-01-31 12:00)
- [x] **Custom title bar theming** — Theme-aware colors, styled buttons. (2026-01-31 12:00)
- [x] **Accessibility & appearance** — WCAG AA contrast, readable fonts. (2026-01-31 12:00)
- [x] **Project structure and naming** — Proper namespace organization. (2026-01-31 12:00)
- [x] **Build configuration** — .NET 8.0, Windows App SDK 1.8.260101001. (2026-01-31 12:00)
- [x] **Debugger configuration** — F5 debugging, launch.json setup. (2026-01-31 12:00)

---

## 📊 Summary

**Total Progress**: 51 completed, 46 incomplete  
**Overall Completion**: 53%

**Current Focus**: Core rename engine implementation  
**Current Version**: `v2026.02.02.05.40.000`

---

## Versioning Scheme

**Format**: `v{YYYY}.{MM}.{DD}.{HH}.{MM}.{SS}.{MSS}`

- **Year (YYYY)**: Development year (2026+)
- **Month (MM)**: Development month (01-12)
- **Day (DD)**: Development day (01-31)
- **Hour (HH)**: Development hour in UTC (00-23)
- **Minute (MM)**: Development minute (00-59)
- **Second (SS)**: Development second (00-59)
- **Millisecond (MSS)**: Development millisecond (000-999)

**Benefits**:

- Automatic versioning with each commit - no manual version bumps needed
- Timestamp-based sorting for version clarity
- Build reproducibility with millisecond precision
- Easy to identify when changes were made

---

This roadmap will be updated as development progresses. Check back frequently for milestones and releases.
