# WinUI 3 Design & Implementation Guide for or1n

## 📋 Overview

This document serves as the definitive reference for WinUI 3 UI/UX implementation in the or1n project. Always follow these patterns when building new features or modifying the UI.

---

## 🎨 Visual Hierarchy & Materials

### Backdrop Materials

**Mica Alt (Primary)** - Used for main application window

- Applied to: `MainWindow` via `SystemBackdrop` property
- Purpose: Strong desktop wallpaper integration for title bar and commanding areas
- Tint: Stronger than standard Mica for better contrast with tabs/menus

**Implementation:**

```csharp
SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
```

**Layering System (3-tier):**

1. **Base Layer:** Mica Alt backdrop (foundation)
2. **Commanding Layer:** `LayerOnMicaBaseAltFillColorDefaultBrush` (navigation, menus)
3. **Content Layer:** `LayerFillColorDefaultBrush` (main content areas)

---

## 🎭 Animation System

### Standard Animation Timings

| Animation Type | Duration | Easing | Use Case |
| --- | --- | --- | --- |
| Fast Entrance | 167ms | `cubic-bezier(0,0,0,1)` | Elements appearing |
| Point-to-Point | 250ms | `cubic-bezier(0.55,0.55,0,1)` | Position/scale changes |
| Fast Exit | 167ms | `cubic-bezier(1,0,1,1)` | Elements disappearing |
| Fade In/Out | 83ms | Linear | Opacity transitions |
| Hover Response | 100ms | `cubic-bezier(0.25,0.1,0.25,1)` | Interactive feedback |

### Page Transitions

- **EntranceNavigationTransitionInfo:** Slide up from bottom (167ms)
- **DrillInNavigationTransitionInfo:** Forward navigation (250ms)
- **SuppressNavigationTransitionInfo:** Instant (no animation)

### Control Animations

- **Buttons:** Hover scale (1.02x), press scale (0.98x)
- **Lists:** Stagger by 40ms between items
- **Panels:** Fade + slide entrance
- **Focus:** 100ms border color transition

---

## 📐 Responsive Design

### Breakpoints

```xml
<VisualStateManager.VisualStateGroups>
    <VisualStateGroup x:Name="WindowSizeStates">
        <VisualState x:Name="CompactMode">
            <VisualState.StateTriggers>
                <AdaptiveTrigger MinWindowWidth="0" />
            </VisualState.StateTriggers>
        </VisualState>

        <VisualState x:Name="MediumMode">
            <VisualState.StateTriggers>
                <AdaptiveTrigger MinWindowWidth="720" />
            </VisualState.StateTriggers>
        </VisualState>

        <VisualState x:Name="WideMode">
            <VisualState.StateTriggers>
                <AdaptiveTrigger MinWindowWidth="1080" />
            </VisualState.StateTriggers>
        </VisualState>
    </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

**Size Classes:**

- **Compact:** 0-719px (phone, narrow windows)
- **Medium:** 720-1079px (tablets, snapped desktop)
- **Wide:** 1080px+ (full desktop windows)

---

## 📏 Spacing System

### Standard Spacing Scale (4px base unit)

```xml
<!-- Consistent spacing throughout app -->
<x:Double x:Key="Spacing.XXSmall">4</x:Double>
<x:Double x:Key="Spacing.XSmall">8</x:Double>
<x:Double x:Key="Spacing.Small">12</x:Double>
<x:Double x:Key="Spacing.Medium">16</x:Double>
<x:Double x:Key="Spacing.Large">20</x:Double>
<x:Double x:Key="Spacing.XLarge">24</x:Double>
<x:Double x:Key="Spacing.XXLarge">32</x:Double>
<x:Double x:Key="Spacing.Huge">40</x:Double>
```

### Margin/Padding Patterns

**Container Padding:**

- Page: 20px all sides (medium screens), 32px (wide screens)
- Card: 16px all sides
- Panel: 12px all sides

**Element Margins:**

- Between sections: 24px vertical
- Between related controls: 8px vertical
- Between control groups: 16px vertical
- Inline controls: 8px horizontal

---

## ♿ Accessibility

### Focus Indicators

**Always implement visible focus:**

```xml
<Style TargetType="Button">
    <Setter Property="UseSystemFocusVisuals" Value="True"/>
    <Setter Property="FocusVisualMargin" Value="-3"/>
</Style>
```

### Text Scaling

**Support system text scaling (100%-225%):**

- Use relative font sizes (not absolute px)
- Test at 150%, 175%, 200%
- Text should never be clipped

### Keyboard Navigation

**Tab Order Rules:**

1. Left-to-right, top-to-bottom
2. Set `TabIndex` only when default order is wrong
3. Use `IsTabStop="False"` for decorative elements

### Screen Reader Support

**Required properties:**

```xml
<Button AutomationProperties.Name="Select Folder"
        AutomationProperties.HelpText="Opens dialog to choose folder">
```

---

## 🎨 Resource Dictionary Structure

### Naming Conventions

**Colors:**

- `{Area}{Element}{Property}Color` → `PageBackgroundColor`
- Always define Light and Dark variants

**Brushes:**

- `{Area}{Element}{Property}Brush` → `PageBackgroundBrush`
- Reference colors: `<SolidColorBrush Color="{ThemeResource PageBackgroundColor}"/>`

**Sizes:**

- `{Control}.{Property}` → `Button.Padding`
- `FontSize.{Size}` → `FontSize.Title`
- `Spacing.{Size}` → `Spacing.Medium`

### Resource Types

**{ThemeResource}** - Dynamic theme-aware (changes with theme)

```xml
<Grid Background="{ThemeResource PageBackgroundBrush}"/>
```

**{StaticResource}** - Static once loaded (spacing, sizes)

```xml
<Button Padding="{StaticResource Button.Padding}"/>
```

---

## 🎯 Control Enhancement Patterns

### Buttons

```xml
<Button Style="{StaticResource AccentButtonStyle}"
        Padding="{StaticResource Button.Padding}"
        UseSystemFocusVisuals="True">
    <Button.Resources>
        <Storyboard x:Name="HoverAnimation">
            <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                           To="1.02" Duration="0:0:0.1"/>
        </Storyboard>
    </Button.Resources>
    <Button.RenderTransform>
        <ScaleTransform/>
    </Button.RenderTransform>
</Button>
```

### Lists

```xml
<ListView ItemsSource="{x:Bind Items}"
          SelectionMode="Single"
          IsItemClickEnabled="True"
          UseSystemFocusVisuals="True">
    <ListView.ItemContainerTransitions>
        <TransitionCollection>
            <EntranceThemeTransition/>
        </TransitionCollection>
    </ListView.ItemContainerTransitions>
</ListView>
```

---

## 🚀 Performance Patterns

### GPU Acceleration

- Enabled by default in WinUI 3
- Use `CompositionTarget.Rendering` for smooth animations
- Avoid layout thrashing (batch property changes)

### Resource Caching

- Static resources cached automatically
- Use `x:Shared="False"` for resources that need per-instance copies
- Dispose unused visual resources in `Unloaded` events

### Virtualization

- ListView/GridView virtualize by default
- Use `ItemsStackPanel` for simple lists
- Use `ItemsWrapGrid` for grid layouts

---

## 📝 Common UI Patterns

### Information Hierarchy

1. **Page Title:** FontSize 28, SemiBold
2. **Section Heading:** FontSize 20, SemiBold
3. **Subsection:** FontSize 16, SemiBold
4. **Body:** FontSize 14, Regular
5. **Caption:** FontSize 12, Regular

### Modal Dialogs

```csharp
ContentDialog dialog = new ContentDialog
{
    Title = "Confirm Action",
    Content = "Are you sure?",
    PrimaryButtonText = "Yes",
    CloseButtonText = "Cancel",
    DefaultButton = ContentDialogButton.Primary,
    XamlRoot = this.Content.XamlRoot
};

await dialog.ShowAsync();
```

### Loading States

```xml
<ProgressRing IsActive="{x:Bind IsLoading, Mode=OneWay}"
              Width="48" Height="48"
              HorizontalAlignment="Center"/>
```

---

## ✅ Pre-Flight Checklist

Before committing UI changes, verify:

- [ ] Mica Alt backdrop active in MainWindow
- [ ] All colors use `{ThemeResource}`
- [ ] All spacing uses spacing system
- [ ] Animations use standard timings
- [ ] Focus indicators visible on all interactive controls
- [ ] Keyboard navigation works (try Tab key)
- [ ] Text readable in Light/Dark/High Contrast
- [ ] Responsive layout tested at 720px, 1080px breakpoints
- [ ] Screen reader announces correctly (test with Narrator)
- [ ] Text scales properly at 150%, 200%

---

## 🔗 Quick Reference

**Apply Mica Alt:**

```csharp
// In MainWindow constructor
SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
```

**Add Page Animation:**

```xml
<Page.Transitions>
    <TransitionCollection>
        <EntranceThemeTransition/>
    </TransitionCollection>
</Page.Transitions>
```

**Focus Visual:**

```xml
<Control UseSystemFocusVisuals="True" FocusVisualMargin="-3"/>
```

**Responsive Grid:**

```xml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="*" MinWidth="320"/>
    <ColumnDefinition Width="2*"/>
</Grid.ColumnDefinitions>
```

---

**Last Updated:** 2026-02-01  
**Version:** 1.0  
**Project:** or1n v1.0 (Shell)
