# Mica / Mica Alt Verification Guide

## Windows 11 Settings Required for Mica

Mica backdrop will NOT show if certain Windows settings are disabled. Check these:

### 1. Transparency Effects (REQUIRED)
**Path**: Settings → Personalization → Colors  
**Setting**: "Transparency effects" must be **ON**

If this is OFF, Mica will not render and you'll only see solid colors.

### 2. Visual Effects (Performance Settings)
**Path**: Control Panel → System → Advanced system settings → Performance Settings  
**Setting**: "Enable transparent selection effects" or use "Let Windows choose what's best"

### 3. Desktop Wallpaper Test
To verify Mica is working:
1. Change your desktop wallpaper to something **bright and colorful** (e.g., a sunset, vibrant landscape)
2. Launch the app
3. The window background should have a **subtle tint/blur** of your wallpaper colors
4. It will be very subtle - not obvious like a full blur

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
