# Mica / Mica Alt Verification Guide

## Windows 11 Settings Required for Mica

Mica backdrop will NOT show if certain Windows settings are disabled. Check these:

### 1. Transparency Effects (REQUIRED) ✅
**Path**: Settings → Personalization → Colors  
**Setting**: "Transparency effects" must be **ON**

If this is OFF, Mica will not render and you'll only see solid colors.

### 2. Visual Effects (Performance Settings) - OPTIONAL
**Path**: Control Panel → System → Advanced system settings → Performance Settings  
**Relevant setting**: "Show translucent selection rectangle"  
**Note**: This doesn't directly affect Mica, but if you have "Adjust for best performance" selected, it disables all effects. Best to use "Let Windows choose what's best" or "Adjust for best appearance"

### 3. Desktop Wallpaper Test (CRITICAL)
To verify Mica is working:
1. Change your desktop wallpaper to something **bright and colorful** (e.g., a sunset, vibrant landscape, bright blue sky)
2. Close and relaunch the app
3. The window background should have a **subtle tint/blur** of your wallpaper colors
4. **THE EFFECT IS VERY SUBTLE** - it's not obvious like a full blur, just a gentle hint of the wallpaper colors

**Important**: Mica Alt uses the wallpaper's **average color** with high blur, so:
- Bright red wallpaper → window has subtle red tint
- Blue sky wallpaper → window has subtle blue tint
- Gray/neutral wallpaper → might be hard to notice the effect

### 4. Code Status
The code IS correctly implemented in `src/MainWindow.xaml.cs` lines 79-96:
```csharp
SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
```

With fallback to DesktopAcrylicBackdrop if Mica not supported.

### 5. How to Verify It's Working
Run this in terminal while app is running:
```powershell
dotnet run
# App should start - check the window background carefully
# Compare to other WinUI 3 apps (Settings app, Calculator)
```

If you see:
- ✅ **Subtle wallpaper tint/blur through window** → Mica is working
- ❌ **Solid flat color, no effect** → Check transparency settings above

### 6. Alternative Test
Open Windows 11 Settings app - it uses Mica Alt. If Settings app shows the effect but yours doesn't, there may be a code issue. If Settings ALSO shows flat colors, transparency is disabled in Windows.

### 7. Quick Debug Test
If you want to test if it's working, try this:
1. Set your wallpaper to **pure red** or **pure blue** (solid color image)
2. Close and relaunch or1n app
3. Look at the window background carefully - you should see a very faint tint of red/blue
4. Switch to Light theme and Dark theme - the tint should be more noticeable in one or the other

**Still not seeing it?** The code might not be applying. Try this terminal command while app is running:
```powershell
Get-Process "or1n-rename-file-name-to-date-created" | Select-Object ProcessName, MainWindowTitle
```

If MainWindowTitle shows up, the window exists. Mica should be active but might just be **extremely subtle** on your wallpaper/theme combination.
