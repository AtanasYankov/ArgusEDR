# Argus EDR v2 GUI Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` (recommended, one subagent per task with review checkpoints) or `superpowers:executing-plans` (inline, batch execution). Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign Argus EDR v2's WPF GUI from a generic Tailwind blue dashboard to a professional black/gold/red identity inspired by Linear.app, matching Argus EDR v1's brand quality.

**Architecture:** Three theme `ResourceDictionary` files (colors, controls, cards) merged in `App.xaml`. MainWindow shrinks to an ~80-line shell. Each screen becomes its own `UserControl` in `Views/`, routed by `ContentControl.DataTemplate` selection on the existing `CurrentView` binding. Zero ViewModel or C# changes — 100% XAML/styling.

**Tech Stack:** WPF (.NET 8), XAML `ResourceDictionary`, CommunityToolkit.Mvvm (unchanged bindings), `x:Static` brush references.

**Design spec:** `docs/superpowers/specs/2026-04-04-argus-gui-redesign-design.md`

**Existing codebase:** `C:\Users\Cayde\Documents\Project Argus\src\Argus.GUI\`

---

## File Map

| File | Action | Lines | Responsibility |
|------|--------|-------|----------------|
| `Themes/ArgusTheme.xaml` | Create | ~80 | All color brushes — foundation for all styles |
| `Themes/Controls.xaml` | Create | ~350 | Button, ToggleButton, ComboBox, TextBox, ScrollViewer styles |
| `Themes/Cards.xaml` | Create | ~120 | Card, Badge/StatusPill styles |
| `App.xaml` | Modify | +3 merges | Merge the 3 theme dictionaries |
| `MainWindow.xaml` | Rewrite | ~80 | Shell: Safe Mode banner, sidebar, ContentControl, status bar |
| `Views/DashboardView.xaml` | Create | ~300 | Dashboard with 3-row card grid + module health + activity |
| `Views/ScannerView.xaml` | Create | ~200 | Two-tab scanner (Deep Scan + Upload) |
| `Views/DefenderView.xaml` | Create | ~150 | Activity log table with filter pills |
| `Views/PrivacyGuardView.xaml` | Create | ~150 | 26 toggles with bulk actions, new toggle style |
| `Views/QuarantineView.xaml` | Create | ~150 | Quarantine table with release/delete |
| `Views/SettingsView.xaml` | Create | ~200 | Tabbed settings (DNS/API/Update) |

**Existing ViewModels (UNCHANGED):**
- `MainViewModel` — `CurrentView` property, `ShowDashboard()`, `ShowPrivacyGuard()`, `ShowSettings()`, `RepairArgus()`, `IsSafeMode`, `SafeModeReason`, `StatusBarText`, `PipeBridgeStatus`
- `DashboardViewModel` — `ProtectionStatus`, `ThreatsDetected`, `FilesScanned`, `QuarantinedItems`, `LastScanTime`, `WatchdogStatus`, `DefenderStatus`, `ServiceLabel`
- `PrivacyGuardViewModel` — `GroupedToggles`, `StatusMessage`, `ApplySelectedCommand`, `Toggle` property in items: `Name`, `Description`, `Enabled`, `WhatBreaks`, `Category`
- `SettingsViewModel` — `DnsProfiles`, `SelectedDnsProfile`, `StatusMessage`, `ApplyDnsCommand`, `SelectedDnsProfile.Primary`, `SelectedDnsProfile.Secondary`, `SelectedDnsProfile.Name`

**Existing Converters (UNCHANGED):** `InverseBoolToVisibilityConverter`, `NavVisibilityConverter`

---

## Task 1: Create Theme Foundation (ArgusTheme.xaml)

**Files:**
- Create: `src/Argus.GUI/Themes/ArgusTheme.xaml`

- [ ] **Step 1: Write ArgusTheme.xaml**

Create the file `src/Argus.GUI/Themes/ArgusTheme.xaml`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- ════════════════════════════════════════════════ -->
    <!-- Background Colors                                -->
    <!-- ════════════════════════════════════════════════ -->
    <SolidColorBrush x:Key="BgPrimary" Color="#0D0D0D"/>
    <SolidColorBrush x:Key="BgSurface" Color="#151515"/>
    <SolidColorBrush x:Key="BgElevated" Color="#1A1A1A"/>
    <SolidColorBrush x:Key="BgSidebar" Color="#0A0A0A"/>
    <SolidColorBrush x:Key="BgNavActive" Color="#151515"/>
    <SolidColorBrush x:Key="BgNavHover" Color="#FFFFFF0D"/>
    <SolidColorBrush x:Key="BgBadgeGreen" Color="#1A3320"/>
    <SolidColorBrush x:Key="BgBadgeGold" Color="#332A00"/>
    <SolidColorBrush x:Key="BgBadgeRed" Color="#331010"/>
    <SolidColorBrush x:Key="BgBadgeGray" Color="#1A1A1A"/>

    <!-- ════════════════════════════════════════════════ -->
    <!-- Accent Colors                                      -->
    <!-- ════════════════════════════════════════════════ -->
    <SolidColorBrush x:Key="AccentGold" Color="#D4AF37"/>
    <SolidColorBrush x:Key="AccentGoldHover" Color="#C6A02D"/>
    <SolidColorBrush x:Key="AccentGoldPressed" Color="#B8941F"/>
    <SolidColorBrush x:Key="AccentGoldDim" Color="#8B7421"/>
    <SolidColorBrush x:Key="AccentRed" Color="#C41E1E"/>
    <SolidColorBrush x:Key="AccentRedHover" Color="#A01A1A"/>
    <SolidColorBrush x:Key="AccentRedCritical" Color="#FF4444"/>

    <!-- ════════════════════════════════════════════════ -->
    <!-- Status Colors                                      -->
    <!-- ════════════════════════════════════════════════ -->
    <SolidColorBrush x:Key="StatusGreen" Color="#2ECC71"/>
    <SolidColorBrush x:Key="StatusGreenDim" Color="#6EE7B7"/>

    <!-- ════════════════════════════════════════════════ -->
    <!-- Border Colors                                      -->
    <!-- ════════════════════════════════════════════════ -->
    <SolidColorBrush x:Key="BorderDefault" Color="#2A2A2A"/>
    <SolidColorBrush x:Key="BorderHover" Color="#3A3A3A"/>
    <SolidColorBrush x:Key="BorderFocus" Color="#D4AF37"/>
    <SolidColorBrush x:Key="BorderError" Color="#C41E1E"/>

    <!-- ════════════════════════════════════════════════ -->
    <!-- Text Colors                                        -->
    <!-- ════════════════════════════════════════════════ -->
    <SolidColorBrush x:Key="TextPrimary" Color="#E8E8E8"/>
    <SolidColorBrush x:Key="TextSecondary" Color="#A0A0A0"/>
    <SolidColorBrush x:Key="TextMuted" Color="#666666"/>
    <SolidColorBrush x:Key="TextInverse" Color="#0D0D0D"/>
    <SolidColorBrush x:Key="TextBadgeGreen" Color="#2ECC71"/>
    <SolidColorBrush x:Key="TextBadgeGold" Color="#D4AF37"/>
    <SolidColorBrush x:Key="TextBadgeRed" Color="#FF4444"/>
    <SolidColorBrush x:Key="TextBadgeGray" Color="#666666"/>

    <!-- ════════════════════════════════════════════════ -->
    <!-- Scrollbar Colors                                   -->
    <!-- ════════════════════════════════════════════════ -->
    <SolidColorBrush x:Key="ScrollbarTrack" Color="#0D0D0D"/>
    <SolidColorBrush x:Key="ScrollbarThumb" Color="#2A2A2A"/>
    <SolidColorBrush x:Key="ScrollbarThumbHover" Color="#3A3A3A"/>

</ResourceDictionary>

- [ ] **Step 2: Create Themes directory**

```bash
mkdir -p "src/Argus.GUI/Themes"
```

- [ ] **Step 3: Build and verify**

After Task 1 file exists, verify no build regressions:
```bash
dotnet build "src/Argus.GUI"
```
Expected: Same result as before (BUILD SUCCEEDED or same warnings as current baseline).

---

## Task 2: Create Controls.xaml (All Component Styles)

**Files:**
- Create: `src/Argus.GUI/Themes/Controls.xaml`

This file defines all WPF control styles as keyed resources:
- `PrimaryButton` (gold bg, black text, 8px radius)
- `SecondaryButton` (transparent bg, gold border on hover)
- `DangerButton` (red bg, white text)
- `SmallButton` (compact secondary)
- `ToggleSwitch` (40x22 track, gold when checked, 200ms animation)
- `DarkScrollBar` (8px width, track #0D0D0D)
- `DarkScrollViewer` (uses DarkScrollBar, auto vertical)

Each style uses {StaticResource} refs to brushes from Task 1's ArgusTheme.xaml.
Key detail: ToggleSwitch uses Storyboard with DoubleAnimation (thumb X:0->18)
and ColorAnimation (track: #2A2A2A -> #D4AF37) on IsChecked trigger.

See the design spec section "3. Component Styles" for exact ControlTemplate details.

- [ ] **Step 2: Verify**

```bash
dotnet build "src/Argus.GUI"
```
Expected: BUILD SUCCEEDED once Themes/ folder and both XAML files exist.

---

## Task 3: App.xaml - Merge Theme Dictionaries

**Files:**
- Modify: `src/Argus.GUI/App.xaml`

Replace current empty App.xaml with one that merges all 3 theme dictionaries.

Order matters: ArgusTheme.xaml first (provides brushes), then Controls.xaml,
then Cards.xaml (both depend on ArgusTheme brushes).

After this step, every View in the app can use {StaticResource AccentGold},
{StaticResource PrimaryButton}, {StaticResource StatCard}, etc.

---

## Task 4: MainWindow.xaml - Rewrite as Shell

**Files:**
- Rewrite: `src/Argus.GUI/MainWindow.xaml`
- Add commands to: `src/Argus.GUI/ViewModels/MainViewModel.cs`

The 685-line file becomes ~80 lines: Safe Mode banner + sidebar + ContentControl + status bar.

### Sidebar
- Gold "ARGUS" text (20px, letter-spacing 2px)
- Dim gold "ENDPOINT DETECTION & RESPONSE" subtitle (8px uppercase)
- Navigation buttons using left gold accent border for active state
- Bottom status card: green/red dot + Watchdog status + module count + version

### Navigation items (6 items)
Current commands: ShowDashboard, ShowPrivacyGuard, ShowSettings
New commands to add: ShowScanner, ShowDefender, ShowQuarantine

Add these to MainViewModel:
1. Properties: `public ScannerViewModel Scanner { get; }`, `DefenderViewModel Defender`, `QuarantineViewModel Quarantine`
2. Inject them in the MainViewModel constructor
3. Commands: `[RelayCommand] private void ShowScanner() => CurrentView = Scanner;` (etc.)
4. Wire to MainWindow nav buttons

### ContentControl
Binds to `Content="{Binding CurrentView}"`.
DataTemplate for each ViewModel type routes to its View UserControl.

---

## Task 5: DashboardView.xaml

**Files:**
- Create: `src/Argus.GUI/Views/DashboardView.xaml`
- Create: `src/Argus.GUI/Views/DashboardView.xaml.cs`

Replaces the inline DataTemplate for DashboardViewModel in MainWindow.xaml.

### Layout (3-row card grid)
- Row 1: 3 equal-width stat cards (PROTECTION/THREATS/QUARANTINED) with 16px gap
- Quick Actions bar: 4 secondary buttons (8px gap)
- Row 2: 2 equal-width stat cards (LAST SCAN/FILES SCANNED)
- Row 3: Split panel - Module Health (Left 40%) + Recent Activity Timeline (Right 60%)

### Bindings (all existing on DashboardViewModel)
- ProtectionStatus, ThreatsDetected, FilesScanned, QuarantinedItems
- LastScanTime, WatchdogStatus, DefenderStatus, ServiceLabel
- Module health: shows Watchdog + Defender status as colored dots + text

### Recent Activity
Display-only placeholder list: timestamp + module name + description.
Real data comes from Phase 10 (integration wiring).

---

## Task 6: PrivacyGuardView.xaml (Redesigned)

**Files:**
- Create: `src/Argus.GUI/Views/PrivacyGuardView.xaml`
- Create: `src/Argus.GUI/Views/PrivacyGuardView.xaml.cs`
- Delete: `src/Argus.GUI/Views/PrivacyGuardTab.xaml` (replaced)

### Binding changes to PrivacyGuardViewModel (src/Argus.GUI/ViewModels/PrivacyGuardViewModel.cs)
Add:
- `ChangesCount` property (int) - count of toggles where Enabled differs from current registry
- `EnableAllCommand` - sets all toggles to Enabled=true
- `DisableAllCommand` - sets all toggles to Enabled=false

### Key UX simplification from v2
Remove inline "WhatBreaks" text from each row. Instead, show it as a tooltip on hover.
This reduces row height by ~70% and makes the page scannable.

### Layout
- Top bar: [Enable All] [Disable All] + "N changes made" indicator + [Apply Privacy Settings]
- Category sections with headers (TELEMETRY, CLOUD, ADVERTISING, NETWORK) separated by #2A2A2A divider
- Toggle rows: Name (13px) + ToggleSwitch (new gold style)
- "Apply Changes" gold primary button, right-aligned

---

## Task 7: ScannerView.xaml

**Files:**
- Create: `src/Argus.GUI/Views/ScannerView.xaml`
- Create: `src/Argus.GUI/Views/ScannerView.xaml.cs`
- Create: `src/Argus.GUI/ViewModels/ScannerViewModel.cs` (placeholder)

### Placeholder ViewModel
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
namespace Argus.GUI.ViewModels;
public sealed partial class ScannerViewModel : ObservableObject
{
    [ObservableProperty] private string _status = "Scanner ready - awaiting IPC integration";
}
```

### Layout
TabControl with two tabs: Deep Scan | Upload
- Deep Scan: scan target options, checkboxes, Start Scan button, progress area, results table
- Upload: drop zone with dashed gold border, uploaded file list

Real data binding comes in Phase 10/Scanner service integration.

---

## Task 8: DefenderView.xaml

**Files:**
- Create: `src/Argus.GUI/Views/DefenderView.xaml`
- Create: `src/Argus.GUI/Views/DefenderView.xaml.cs`
- Create: `src/Argus.GUI/ViewModels/DefenderViewModel.cs` (placeholder)

### Layout
- Header: "Defender" + subtitle + status badge
- Filter pills: [All] [Malicious] [Suspicious] [Clean]
- Full-width DataGrid: TIME | TYPE | EVENT | THREAT LEVEL
- Row click expands inline details

---

## Task 9: QuarantineView.xaml

**Files:**
- Create: `src/Argus.GUI/Views/QuarantineView.xaml`
- Create: `src/Argus.GUI/Views/QuarantineView.xaml.cs`
- Create: `src/Argus.GUI/ViewModels/QuarantineViewModel.cs` (placeholder)

### Layout
- Header: "Quarantine" + item count badge
- Full-width DataGrid with empty state
- Row selection highlights with gold left border
- Detail panel below: file hash, detection reason, timestamp
- Bottom: [Release Selected] (gold outline) + [Delete Permanently] (danger red)

---

## Task 10: SettingsView.xaml

**Files:**
- Create: `src/Argus.GUI/Views/SettingsView.xaml`
- Create: `src/Argus.GUI/Views/SettingsView.xaml.cs`

### Existing bindings preserved
- DnsProfiles, SelectedDnsProfile (with Primary/Secondary/Name), StatusMessage, ApplyDnsCommand

### Layout - 3 tabs
- DNS: Toggle + dropdown + IP display + [Apply DNS Settings]
- API Keys: Service | Key (masked) | Status table + [Add key] button
- Auto-Update: Policy options (Minor/Major/Critical)

---

## Task 11: Final Polish & Verification

- [ ] Remove old PrivacyGuardTab.xaml from project
- [ ] Remove inline DataTemplates from MainWindow.xaml (replace with View UserControl references)
- [ ] Verify all 6 nav items switch views correctly
- [ ] Verify Safe Mode banner toggles correctly
- [ ] Verify status bar shows pipe status
- [ ] Screenshot each screen for before/after comparison
