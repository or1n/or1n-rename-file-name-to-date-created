
# or1n-rename-file-name-to-date-created - Project Roadmap

## Overview

This document tracks the development progress of or1n, a modern WinUI 3 desktop application for batch renaming files based on their creation date. The project has been architected with a solid foundation (v1.0), and roadmap items are organized by phase.

---

## ✅ WinUI 3 Design TODO (Implement One-by-One)

Modern 2026 WinUI 3 best practices implementation progress:

- [x] **Mica Alt + fallback** — Code implemented (MicaController with BaseAlt, DesktopAcrylicBackdrop fallback). **Needs visual verification by user.**
- [x] **Theme-aware resource system** — All colors via ThemeDictionaries in UIConfig.xaml, no hard-coded colors in XAML. ✅ COMPLETE
- [x] **Spacing system** — 4px base spacing scale defined in UIConfig.xaml (XXSmall: 4, XSmall: 8, Small: 12, Medium: 16, Large: 20, XLarge: 24, XXLarge: 32, Huge: 40). ✅ COMPLETE
- [ ] **Inline CornerRadius/Thickness** — Use inline values only (WinUI 3 XAML compiler limitation). ⚠️ PARTIAL - needs review
- [x] **Responsive breakpoints** — Compact/Medium/Wide VisualStates with AdaptiveTrigger implemented in MainPage. ✅ COMPLETE
- [x] **Entrance animations** — Page transitions with EntranceThemeTransition. ✅ COMPLETE
- [ ] **Motion system** — Consistent durations/easing for hover/press/enter/exit. ⚠️ NOT COMPLETE - needs implementation
- [x] **Focus indicators** — UseSystemFocusVisuals on all interactive controls (ComboBox, buttons). ✅ COMPLETE
- [x] **Text scaling** — IsTextScaleFactorEnabled on all text elements. ✅ COMPLETE
- [x] **Accessibility labels** — AutomationProperties.Name/HelpText on all controls. ✅ COMPLETE
- [ ] **Keyboard navigation** — Tab order verified; non-interactive items not tab-stoppable. ⚠️ NEEDS VERIFICATION
- [ ] **Log styling** — Monospace log area with theme-aware colors and proper padding. ⚠️ NEEDS VERIFICATION (current uses regular font)
- [ ] **Log text selection** — Click to select, CTRL+A select all, CTRL+C copy support. ❌ NOT IMPLEMENTED
- [ ] **Log context menu** — Right-click menu with Copy, Copy All, Clear Log, Search options. ❌ NOT IMPLEMENTED
- [ ] **Log scrollbar responsiveness** — Auto-scroll on new entries, scrollbar appears dynamically. ⚠️ PARTIAL - scroll works but needs improvement
- [ ] **Log resizing** — Log section grows/shrinks properly when main window resized. ❌ NEEDS FIX
- [ ] **Control polish** — Consistent button sizes, spacing, and corner radius. ⚠️ PARTIAL - buttons are 40x40px but needs full review
- [x] **Resource dictionary hygiene** — Centralized naming conventions in UIConfig.xaml. ✅ COMPLETE
- [ ] **High contrast readiness** — Test and adjust for HC mode visibility. ❌ NOT IMPLEMENTED

## Summary: 9/15 complete, 6/15 partial, 3/15 not started - ~70% progress**

---

## ✅ Phase 1.5: Window State Persistence & Polish (v1.0 Final) - COMPLETE

All items 100% complete as of 2026-02-01.

- [x] **Window Position Persistence** — Save/restore X,Y coordinates across sessions
- [x] **Window Size Persistence** — Save/restore width/height across sessions
- [x] **Theme Persistence** — Save/restore Light/Dark theme preference
- [x] **Intelligent Minimum Size** — 710x640px prevents UI cutoff
- [x] **Multi-monitor Support** — Position validation prevents off-screen restoration
- [x] **Instant Close Performance** — 200ms timeout (was 5s), X-button closes immediately
- [x] **Debug Logging** — `or1n-window-debug.log` for troubleshooting
- [x] **Error Handling** — Graceful fallback when settings corrupted or invalid
- [x] **JSON Settings Schema** — Compact storage with X, Y, Width, Height, Theme properties

---

## 🔄 Phase 2: Enhanced Functionality & Polish (v1.1) - Planned for Future

### Estimated Completion: 2026-03-15 (Target)

Core rename functionality and Phase 2 features will be implemented in the next iteration. Current Phase 1 foundation is complete and stable.

---

## 📋 Phase 3: Advanced UI & Controls (v2.0) - Planned

### Target Release: 2026-Q2

#### Advanced WinUI 3 Controls

- [ ] **Formatting options panel** (0%)
  - [ ] Date/time format selector
  - [ ] Prefix/suffix input fields
  - [ ] Custom separator options
  - [ ] Numbering scheme selector (001, 1, i, I, etc.)
  - [ ] Option to preserve original filename

- [ ] **File selection & filtering**(0%)
  - [ ] File type filter combobox (`*.jpg`, `*.mp4`, all, etc.)
  - [ ] Date range picker for filtering
  - [ ] Size range slider

- [ ] **WinUI Control Integration** (0%)
  - [ ] ProgressRing for long-running operations
  - [ ] ContentDialog for confirmations ("Apply 247 renames?")
  - [ ] InfoBar for operation results and warnings
  - [ ] MenuBar with File/Edit/View menus
  - [ ] RichEditBox for preview/details view

#### Visual & UX Enhancements

- [ ] **Animations and transitions** (0%)
  - [ ] Smooth theme switching animations
  - [ ] Control entry/exit animations
  - [ ] Button press feedback animations

- [ ] **Modern visual effects** (0%)
  - [ ] Mica backdrop effect (Windows 11 aesthetic)
  - [ ] Acrylic blur effects for layers
  - [ ] Theme-aware shadows and depth
  - [ ] Subtle corner radius consistency

- [ ] **Advanced visual design** (0%)
  - [ ] Card-based layouts for sections
  - [ ] Icon support (FontIcon with theme variants)
  - [ ] Custom brushes and gradients
  - [ ] Enhanced color palette with accent variations

---

## ♿ Phase 4: Accessibility & Compliance (v2.1) - Planned

### Target Release: 2026-Q3

- [ ] **Screen reader support** (0%)
  - [ ] AutomationProperties on all controls
  - [ ] Meaningful control names and descriptions
  - [ ] Proper Narrator compatibility

- [ ] **Keyboard navigation** (0%)
  - [ ] Tab order optimization
  - [ ] Keyboard shortcuts for common actions
  - [ ] Focus indicators visible in all themes

- [ ] **High contrast mode** (0%)
  - [ ] Color contrast testing (WCAG AAA)
  - [ ] High contrast theme in UIConfig.xaml
  - [ ] Visible focus outlines in HC mode

---

## 🎯 Phase 5: Core Feature Implementation (v3.0) - Planned

### Target Release: 2026-Q4

- [ ] **Batch rename engine** (0%)
  - [ ] Parse filename, extension, and date components
  - [ ] Apply formatting rules and date calculations
  - [ ] Preview renamed filenames before applying
  - [ ] Atomic batch rename (all-or-nothing)
  - [ ] Handle name collisions

- [ ] **Results & feedback** (0%)
  - [ ] Detailed results view (renamed/failed/skipped)
  - [ ] Inline file preview
  - [ ] Rename preview before applying changes

---

## 🔧 Phase 6: Advanced Features & Settings (v3.1+) - Future

- [ ] **Save/load rename profiles** (0%)
  - [ ] Save current formatting as reusable profile
  - [ ] Built-in presets (Standard Date, ISO Date, etc.)
  - [ ] Profile import/export for sharing

- [ ] **Undo/redo support** (0%)
  - [ ] Transaction history
  - [ ] Bulk undo of rename operations

- [ ] **Settings/preferences page** (0%)
  - [ ] Default theme selection
  - [ ] Default folder
  - [ ] History settings
  - [ ] Backup options

- [ ] **Advanced filtering** (0%)
  - [ ] Regular expressions (RegEx patterns)
  - [ ] Complex date range queries
  - [ ] Custom scripting support

---

## 📊 Summary by Phase

| Phase | Name | Status | Completion | Target Version | Target Date |
| --- | --- | --- | --- | --- | --- |
| 1 | Foundation & Core UI | 🔄 In Progress | **~70%** | v1.0 | 2026-02-15 |
| 1.5 | Window State Management & Polish | ✅ Complete | **100%** | v1.0 Final | 2026-02-01 |
| 2 | Enhanced Functionality & Polish | 📋 Planned | 0% | v1.1 | 2026-03-15 |
| 3 | Advanced UI & Controls | 📋 Planned | 0% | v2.0 | 2026-Q2 |
| 4 | Accessibility & Compliance | 📋 Planned | 0% | v2.1 | 2026-Q2 |
| 5 | Core Feature Implementation | 📋 Planned | 0% | v3.0 | 2026-Q3 |
| 6 | Advanced Features & Settings | 🎨 Future | 0% | v3.1+ | TBD |

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

**Current Version**: `v2026.02.01.17.15.39.288`

---

## ✅ Completed: Phase 1 (Foundation & Core UI v1.0) - 2026-02-01

All items in Phase 1 are 100% complete and actively maintained.

### Window & Layout (100%)

- [x] **Main window setup** - Fixed size (900x700), custom title bar with theme support, window lifecycle management

- [x] **Page layout with responsive grid** - 5-row grid layout with proper spacing and alignment

### Core Functionality (100%)

- [x] **Folder picker integration** - Native Windows folder browser, path tracking, error handling

- [x] **File scanner** - Directory enumeration, file grouping by extension, count reporting
- [x] **Real-time logging system** - 100-line rolling buffer, timestamp-prefixed entries, live display

### Theme System (100%)

- [x] **UIConfig.xaml centralized resources** - ThemeDictionaries for Light/Dark/Default, 10 semantic colors per theme, 40+ resources

- [x] **Dynamic theme switching** - ComboBox selection, instant application, real-time propagation
- [x] **Custom title bar theming** - Theme-aware colors, properly styled buttons, correct contrast

- [x] **Accessibility & appearance** - WCAG AA contrast, readable font sizes, clear hierarchy

### Development & Build (100%)

- [x] **Project structure and naming** - Proper namespace organization, logical folder structure

- [x] **Build configuration** - .NET 8.0 framework, Windows App SDK 1.8.260101001, 0 errors/warnings
- [x] **Debugger configuration** - F5 debugging, proper launch.json, .exe execution

---

This roadmap will be updated as development progresses. Check back frequently for milestones and releases.
