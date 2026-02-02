# Setup Guide for or1n

**or1n** requires Windows 11 with .NET 8.0+ SDK and a modern code editor. This guide walks you through getting the project running.

## System Requirements

**Minimum:**

- Windows 11 (Build 19041 or later)
- .NET SDK 8.0 or later

**Recommended:**

- Latest Windows 11 build
- .NET SDK 8.0.417 or later

## IDE/Editor Options

### Option 1: VS Code (Recommended for Lightweight Development)

**Advantages**: Fast, customizable, minimal overhead

1. **Install [VS Code](https://code.visualstudio.com/)**
2. **Install required extensions**:
   - [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
   - [XAML Language Support](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.xaml)
   - [.NET Runtime](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime)

### Option 2: Visual Studio 2022 (Recommended for Advanced Debugging)

**Advantages**: Integrated debugger, native XAML designer, better IntelliSense

1. **Install [Visual Studio 2022 Community](https://visualstudio.microsoft.com/)**
2. **Select workload**: ".NET desktop development" (includes WinUI 3 templates)
3. **Install components**:
   - .NET Desktop Development workload
   - Windows 10/11 SDK
   - C++ build tools (optional, for native debugging)

## Installation Steps

### Method 1: Automated Setup (Windows PowerShell)

```powershell
# Navigate to project folder
cd path\to\or1n_rename_file-names-to-date

# Run the install script
.\install.cmd

```

The script will:

- ✅ Verify .NET SDK is installed
- ✅ Check .NET version
- ✅ Restore NuGet packages
- ✅ Build the project
- ✅ Display next steps

### Method 2: Manual Setup

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the app (choose one):
dotnet run
# OR for watch mode (auto-rebuild):
dotnet watch run

```

### Method 3: Using Your IDE

**VS Code:**

1. Open the project folder in VS Code
2. Press `Ctrl+Shift+B` or run **build** task
3. Press `F5` to debug

**Visual Studio 2022:**

1. Open `or1n-rename-file-name-to-date-created.slnx`
2. Press `Ctrl+Shift+B` to build
3. Press `F5` to debug

## Verification

After installation, verify the setup:

```bash
# Check .NET is installed
dotnet --version

# Check SDK version
dotnet --list-sdks

# Run the app
dotnet run --project or1n-rename-file-name-to-date-created.csproj

```

You should see a window with:

- Title: "or1n Rename File Name To Date Created"
- Subtitle: "A modern WinUI 3 app..."

- Theme selector (System/Light/Dark)
- Open and Scan buttons

- Log area at bottom

## Development Dependencies

These are installed via NuGet (see the .csproj for exact versions):

- Microsoft.WindowsAppSDK
- Microsoft.Web.WebView2
- Microsoft.Windows.SDK.BuildTools

## Troubleshooting

### ".NET SDK not found"

- Download and install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Restart your terminal/IDE after installation

### Build fails with "Project not found"

- Ensure you're in the correct directory
- Verify the `.csproj` file exists

- Run `dotnet restore` first

### "WinRT not supported on this platform"

- This project requires **Windows 11**
- You cannot run this on Linux or macOS

- Use Windows 11 Build 19041 or later

### XAML errors in VS Code

- These are often false positives due to VS Code's limited XAML support
- The build still succeeds (ignore them)

- Use Visual Studio 2022 for better XAML debugging

### App won't launch in debugger

- Ensure launch.json launches the `.exe` (not `.dll`)
- See [WORKFLOW.md](WORKFLOW.md) for details

## Next Steps

- Overview: [README.md](README.md)
- Build/run workflow: [WORKFLOW.md](WORKFLOW.md)
- Current work list: [TODO.md](TODO.md)

## Support

For issues:

1. Check [WORKFLOW.md](WORKFLOW.md) troubleshooting section
2. See [FILES_AND_FOLDERS.md](FILES_AND_FOLDERS.md) for project structure
3. Read `.github/copilot-instructions.md` for development guidelines
4. Search [WinUI 3 Documentation](https://learn.microsoft.com/windows/apps/winui/winui3/)

---

Happy coding! 🚀
