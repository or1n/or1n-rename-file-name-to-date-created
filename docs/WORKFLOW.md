# Development Workflow for or1n

## Quick Build & Run

```bash
# Standard build and run
dotnet build
dotnet run

# Watch mode (auto-rebuild on file changes)
dotnet watch run

# Deep clean before build (if you have build issues)
.\clean_winui.ps1  # Then: dotnet build

```

## Common Tasks

### Debugging

- Press **F5** in VS Code or Visual Studio to start debugging
- The app launches as `.exe` (not `.dll`) - this is required for WinUI 3

- Set breakpoints in C# code or XAML code-behind files

### XAML Theme Changes

- Edit `UIConfig.xaml` for colors, font sizes, margins, etc.
- Changes apply immediately during debugging (no rebuild needed for most theme changes)

- Both light and dark themes are defined in `ResourceDictionary.ThemeDictionaries`

### Adding New Views

1. Create new `.xaml` and `.xaml.cs` files in the `Views/` folder
2. Add them to `App.xaml` resource merging if needed
3. Navigate to them from `MainWindow.xaml.cs` using `RootFrame.Navigate(typeof(YourNewPage))`

### Testing Theme Switching

1. Launch the app (F5)
2. Use the ComboBox dropdown to switch between "System theme", "Light theme", "Dark theme"
3. All UI colors should update instantly, including the title bar
4. Close and reopen the app - your theme preference is saved and restored

### Testing Window State Persistence

1. Launch the app (F5)
2. Drag the window to a different position on screen
3. Resize the window to a custom size
4. Close the app
5. Reopen the app - window position and size are restored
6. Switch theme, close, reopen - theme preference is restored

**Settings Storage:**
- Location: `C:\Users\[User]\AppData\Local\Or1nRenameFileNameToDate\`
- File: `window-settings.json` (position, size, theme)
- Debug: `or1n-window-debug.log` (troubleshooting logs)

## Troubleshooting

### Window State Not Persisting

- Check that `LocalAppData\Or1nRenameFileNameToDate\window-settings.json` exists and is valid JSON
- Open `or1n-window-debug.log` to see detailed save/load lifecycle
- Try deleting the settings file and relaunching - app will create fresh defaults and center window

### Position or Size Resets on Launch

- Check `or1n-window-debug.log` for `IsValidPosition: failed` messages
- This indicates the saved position was off-screen or on a display that's no longer connected
- App should auto-restart and center on the current display next time

### Build Fails

```powershell
# Run the deep clean script
.\clean_winui.ps1
dotnet clean
dotnet restore
dotnet build

```

### IntelliSense Shows False Errors (CS0103, CS1061)

- These are known XAML code-generation issues in VS Code
- They don't prevent compilation - you can safely ignore them

- The pragmas in `App.xaml.cs` and `MainWindow.xaml.cs` suppress these warnings

### Title Bar Not Theming Correctly

- Ensure `MainWindow.xaml.cs` has called `SetupTitleBar()` in the constructor
- Check that `RootFrame.ActualThemeChanged` event is registered

- Verify theme colors in `UIConfig.xaml` are correct for your target theme

### Visual Studio vs VS Code

- **VS Code** (recommended): Lighter, faster, highly customizable
- **Visual Studio 2022**: Heavier but better debugger and IntelliSense

- Use whichever you're most comfortable with

## Code Organization

```text
Views/                    - UI pages (MainPage, future pages)
Helpers/                  - Utility classes
  ├── WindowHelper.cs     - Window tracking and communication
  └── WindowSettings.cs   - Persistent window state (position, size, theme)
Assets/                   - Images, icons, app branding
bin/, obj/                - Build artifacts (generated, ignore in git)
Properties/               - Project properties (mostly empty for WinUI 3)
UIConfig.xaml             - Centralized theme/style resources
App.xaml.cs               - Application lifecycle
MainWindow.xaml.cs        - Window setup (title bar, frame navigation, persistence)
Program.cs                - Bootstrap entry point
```

## Performance Notes

- **Cold Start**: ~1-2 seconds (normal for .NET WinUI 3 apps)
- **Window Restoration**: <50ms (loads position/size/theme from JSON cache)
- **Close Performance**: 200ms timeout (instant visual close, async settings save)
- **Folder Scanning**: Currently lists files by type; scale considerations TBD
- **Theme Switching**: Instant (no processing, just UI refresh + async save)
- **Memory**: ~80-120 MB typical (WinUI 3 base overhead + .NET runtime)

## Contributing

- Follow C# naming conventions: `PascalCase` for classes/methods, `camelCase` for variables

- Use theme resources from `UIConfig.xaml` - never hardcode colors
- Add XML documentation comments to public methods

- Test your changes in both light and dark themes

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/windows/apps/winui/winui3/)

- [XAML Overview](https://learn.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/)
