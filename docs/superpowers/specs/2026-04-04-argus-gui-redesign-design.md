# Argus EDR v2 GUI Redesign — Design Specification

**Date:** 2026-04-04  
**Status:** Draft — awaiting user approval  
**Scope:** Visual redesign only — zero C#/ViewModel changes  

---

## Context

Argus EDR v2's current GUI uses a Tailwind Slate blue color palette (`#3B82F6` accent on `#0F172A` background) that looks like a generic admin dashboard template, not a professional security tool. Argus EDR v1 had a more professional identity with black backgrounds and gold (`#D4AF37`) accents. This redesign restores that identity, elevates it to "professional grade" (inspired by Linear.app's calm, purposeful design language), and implements all missing views (Scanner, Defender, Quarantine, Settings as full screens).

**Design references:** Argus EDR v1 UI screenshots (gold/black brand identity), Linear.app (layout discipline, component patterns, calm aesthetic).

**Color system:** Black (`#0D0D0D`), Gold (`#D4AF37`), Red (`#C41E1E`), with neutral grays for text hierarchy.

---

## 1. Color Palette

### Color Tokens

| Token | Hex | Purpose |
|-------|-----|---------|
| `bg-primary` | `#0D0D0D` | Window background |
| `bg-surface` | `#151515` | Card backgrounds |
| `bg-elevated` | `#1A1A1A` | Elevated surfaces (inputs, sub-cards) |
| `bg-sidebar` | `#0A0A0A` | Sidebar (darker than main to recede) |
| `bg-nav-active` | `#151515` | Active nav item background |
| `bg-nav-hover` | `#FFFFFF0D` | Nav hover background overlay |
| `bg-badge-green` | `#1A3320` | Green status badge background |
| `bg-badge-gold` | `#332A00` | Gold status badge background |
| `bg-badge-red` | `#331010` | Red status badge background |
| `bg-badge-gray` | `#1A1A1A` | Gray/disconnected badge background |
| `accent-gold` | `#D4AF37` | Primary brand color (buttons, borders, focus) |
| `accent-gold-hover` | `#C6A02D` | Gold hover state |
| `accent-gold-pressed` | `#B8941F` | Gold pressed state |
| `accent-gold-dim` | `#8B7421` | Muted gold for secondary accents |
| `accent-red` | `#C41E1E` | Danger actions, Safe Mode banner |
| `accent-red-hover` | `#A01A1A` | Red hover state |
| `accent-red-critical` | `#FF4444` | Critical alerts, threat indicators |
| `status-green` | `#2ECC71` | Healthy/active status |
| `status-green-dim` | `#6EE7B7` | Green sub-text |
| `border` | `#2A2A2A` | Subtle card borders |
| `border-hover` | `#3A3A3A` | Card border on hover |
| `border-input` | `#2A2A2A` | Input default border |
| `border-focus` | `#D4AF37` | Input focus (gold) |
| `border-error` | `#C41E1E` | Input error state |
| `text-primary` | `#E8E8E8` | Headings, labels, primary text |
| `text-secondary` | `#A0A0A0` | Subtitles, descriptions |
| `text-muted` | `#666666` | Timestamps, lowest emphasis |
| `text-inverse` | `#0D0D0D` | Text on light backgrounds (e.g., on gold buttons) |
| `text-badge-green` | `#2ECC71` | Green badge text |
| `text-badge-gold` | `#D4AF37` | Gold badge text |
| `text-badge-red` | `#FF4444` | Red badge text |
| `text-badge-gray` | `#666666` | Gray badge text |
| `scrollbar-track` | `#0D0D0D` | Scrollbar track |
| `scrollbar-thumb` | `#2A2A2A` | Scrollbar thumb |
| `scrollbar-thumb-hover` | `#3A3A3A` | Scrollbar thumb hover |

### What Changes from v2

| Removed | Replaced By | Reason |
|---------|------------|--------|
| `#3B82F6` (blue accent everywhere) | `#D4AF37` (gold) | Blue has no brand meaning; gold is Argus's identity |
| `#0F172A` (slate-900 background) | `#0D0D0D` (near-black) | Slate blue undertone reads as template; black is premium |
| `#1E293B` (slate-800 cards) | `#151515` (dark surface) | Removes blue-gray tinge from card surfaces |
| `#64748B` (text-muted) | `#666666` (neutral gray) | Slate blue-tinted gray is hard to read on dark |
| `#94A3B8` (text-secondary) | `#A0A0A0` (neutral gray) | Improves readability from 4.3:1 to 7:1 contrast on black |
| `#334155` (borders) | `#2A2A2A` (neutral dark) | Removes blue tint from card borders |
| `#991B1B` (safe mode red) | `#C41E1E` (brighter professional red) | More urgent, higher contrast |
| Emoji `⚠` in Safe Mode banner | Plain text only | Professional tools avoid emoji; plain text is more serious |

---

## 2. Layout & Navigation

### Window Structure

```
┌──────────────────────────────────────────────────────┐
│  SAFE MODE BANNER (full-width #C41E1E, only if active)│
├──────────┬───────────────────────────────────────────┤
│          │                                           │
│  SIDEBAR │  CONTENT AREA (24px top, 32px sides)      │
│  (260px) │  ┌─────────────────────────────────────┐  │
│          │  │  Page Title  +  Status Badge         │  │
│  Logo    │  │  Page subtitle                       │  │
│          │  └─────────────────────────────────────┘  │
│  Nav 1   │                                           │
│  Nav 2   │  ┌─────────────────────────────────────┐  │
│  Nav 3   │  │  Content Cards                      │  │
│  Nav 4   │  │  (grid layout, #2A2A2A borders)     │  │
│  Nav 5   │  └─────────────────────────────────────┘  │
│  Nav 6   │                                           │
│  Nav 7   │  ┌─────────────────────────────────────┐  │
│          │  │  Full-width Panel (tables, lists)   │  │
│  Status  │  │                                     │  │
│  Card    │  └─────────────────────────────────────┘  │
│          │                                           │
├──────────┴───────────────────────────────────────────┤
│  STATUS BAR (28px: pipe status left, version right)   │
└──────────────────────────────────────────────────────┘
```

### Sidebar Navigation (7 items)

1. **Dashboard** — Overview of all modules
2. **Scanner** — Deep Scan + File Upload analysis
3. **Defender** — Real-time protection activity log
4. **Privacy Guard** — 26-toggle Windows hardening
5. **Quarantine** — Isolated files awaiting review
6. **Settings** — DNS, API keys, update policy
7. (separator line) — Settings at bottom, visually separated

### Active Navigation State (Linear-Inspired)

Instead of v2's background highlight, use a **left accent border**:

- Active item: 3px gold left border (`#D4AF37`), `#151515` background, `#E8E8E8` text, SemiBold
- Inactive item: No border, transparent background, `#A0A0A0` text, regular weight
- Hover: `#FFFFFF0D` background, text brightens to `#C8C8C8`
- Animation: Gold border `TranslateTransform.Y` to 150ms when switching items

### Sidebar Brand & Status

```
ARGUS                    ← Gold (#D4AF37), bold 20px, letter-spacing 2px
ENDPOINT DETECTION       ← Dim gold (#8B7421), 8px, uppercase
& RESPONSE

[navigation items...]

─────────────────────

  Healthy              ← Status dot + Watchdog status
  4 modules active     ← Module count
  v2.1.0 build 1447    ← Version in #666
```

### SideBar Bottom Status Card

- Dot indicator: Green `#2ECC71` (healthy) or Red `#FF4444` (compromised)
- Watchdog status text (healthy/compromised/disconnected)
- Module count (how many of 4 are running)
- Version + build number

### Safe Mode Banner

- Full-width, `#C41E1E` background, no transparency
- Bold text: "SAFE MODE — Argus integrity compromised: {reason}"
- "Repair Argus" button (gold background `#D4AF37`, black text `#0D0D0D` for contrast on red)
- No emoji, no icon — plain text only
- Height: 44px (fixed, not auto)

### Content Page Headers

Every page follows this consistent header pattern:

```
Page Title                          [Status Badge]
Short descriptive subtitle

───────────────────────────────────────────── (thin #2A2A2A separator)
```

- Title: 24px, `#E8E8E8`, Bold
- Subtitle: 13px, `#666666`
- Status Badge: pill (8px radius, colored bg + colored text)
- Separator: 1px `#2A2A2A`, 16px below subtitle

---

## 3. Component Styles

### Buttons

| Type | Background | Border | Text | Radius | Padding | Use |
|------|-----------|--------|------|--------|---------|-----|
| Primary | `#D4AF37` → `#C6A02D` | None | `#0D0D0D` | 8px | 14 24 | Scan, Apply, Save, Run |
| Secondary | `#0D0D0D` | 1px `#2A2A2A` | `#E8E8E8` | 8px | 14 24 | Cancel, Back, Reset |
| Danger | `#C41E1E` → `#A01A1A` | None | #FFFFFF | 8px | 14 24 | Delete, Release threat, Exit safe mode |
| Small | `#0D0D0D` | 1px `#2A2A2A` | `#E8E8E8` | 6px | 8 12 | Inline actions, badges with click |

**Button interaction states:**
- Hover: Background changes (see table), secondary button border → `#D4AF37`
- Pressed: Darken by 8%, slight scale-down (none if animation budget exceeded)
- Disabled: Opacity 0.4, no hover effect
- Font: 13px, SemiBold (Segoe UI Variable)

### Status Badges/Pills

| State | Background | Text | Dot | Example |
|-------|-----------|------|-----|---------|
| Active/Healthy | `#1A3320` | `#2ECC71` | ● Solid green | Watchdog: Healthy |
| Scanning/Warning | `#332A00` | `#D4AF37` | ● Pulsing gold | Scanner: Scanning |
| Compromised/Danger | `#331010` | `#FF4444` | ● Solid red | Compromised |
| Disconnected/Neutral | `#1A1A1A` | `#666666 | None | Service: Disconnected |

- Height: 24px, Padding: 6px 12px, Font: 11px, Radius: 8px
- Pill pattern from Linear: colored background + matching text color
- No emoji in status text — pure text is more professional

### Card Component

```
┌──────────────────────────────────┐
│  THREATS DETECTED                │
│                                  │
│  12                              │
│                                  │
│    ╭──╮ Auto-quarantine enabled  │
│    ╰──╯                          │
└──────────────────────────────────┘
```

- `#151515` background, `#2A2A2A` border, 1px solid
- `Radius: 12px` on all cards
- Hover: border → `#3A3A3A` (100ms transition)
- Content padding: 20px horizontal, 16px vertical
- Card label: 10px, uppercase, `#666666`, Bold, letter-spacing: 1px
- Card value: 28px, Bold, `#E8E8E8` (or status color)
- Sub-badge: 10px, on `#1A1A1A` bg with radius 6px, 8 4 padding

### Toggle Switch (Privacy Guard)

```
Inactive: ───○              Track: #2A2A2A, Thumb: #A0A0A0
Active:       ───●         Track: #D4AF37, Thumb: #0D0D0D (dark for contrast)
```

- Track width: 40px height 22px (smaller than v2's 48x26 — more compact)
- Thumb diameter: 18px
- Animation: 200ms ease (keep v2's Smooth slide)
- Gold track with dark thumb = high-contrast "on" signal

### Input Fields

```
Default:  ┌──────────────────┐  Border: 1px #2A2A2A, bg #1A1A1A, text #E8E8E8
          │ Placeholder text │  Height: 38px, Padding: 10 14, Radius: 8px
          └──────────────────┘

Focused:  ┌──────────────────┐  Border: 1px #D4AF37 (gold focus ring)
          │ User input...    │  Same bg/text, same height/radius
          └──────────────────┘

Error:    ┌──────────────────┐  Border: 1px #C41E1E (red border)
          │ Invalid input    │  Same bg/text, same height/radius
          └──────────────────┘
```

### Tables / DataGrid

| Cell padding: 12 16
- Header row: `#151515` bg, 10px uppercase `#666666` Bold, sticky top
- Row separators: 1px `#2A2A2A` (only horizontal, no vertical grid lines)
- Row hover: `#151515` background (no zebra striping — cleaner on dark)
- Severity column: colored pill badge
- Clicking a row expands inline details below (file hash, full command-line, parent process chain)

### Scrollbar

- Track: `#0D0D0D`, Thumb: `#2A2A2A`, width: 8px
- Thumb hover: `#3A3A3A`
- Corner: no corner button/resize handle (minimalist)
- Applies to all ScrollViewer instances via style setter

---

## 4. Screen Layouts

### 4.1 Dashboard

**Row 1** — Three equal-width stat cards (16px gap):
- PROTECTION: Value `ACTIVE` (green), sub-badge "Real-time monitoring active"
- THREATS DETECTED: Value number (red), sub-badge "Auto-quarantine enabled"
- QUARANTINED: Value number (gold), sub-badge "Encrypted safe storage"

**Row 2** — Two equal-width stat cards (16px gap):
- LAST SCAN: Timestamp value, 18px `#E8E8E8`
- FILES SCANNED: Count value, 18px `#E8E8E8`

**Row 3** — Split panel:
- Left (40%): Module Health — Watchdog, Defender, Engine, Recovery. Vertical list with colored dots. Each module is a row: dot (10px), name (13px SemiBold), status text (11px muted). 12px spacing.
- Right (60%): Recent Activity timeline — last 5-6 events. Each row: timestamp (10px monospace, `#666666`), module name (11px, `#8B7421`), description (12px, `#E8E8E8`).

**Quick Actions Bar** (between row 1 and 2):
- [ Run Deep Scan ] [ Upload File ] [ View Quarantine ] [ Open Settings ]
- All secondary buttons, 8px gap between

**Empty States:**
- Centered, muted text, subtle diamond icon, clear action prompt

### 4.2 Scanner (Two Tabs)

**Tab Navigation:** Deep Scan | Upload (pill-style tabs like Linear, active = gold text + gold underline)

**Deep Scan Tab:**
- Scan Target: Radio-style selection (Quick Scan, Full System Scan, Custom Path) + file/folder picker
- Options: Checkboxes (Include archives, YARA rules, VirusTotal, OTX)
- [ Start Scan ] primary gold button
- Progress section: progress bar (#2A2A2A track, #D4AF37 fill, 6px height, 4px radius), file count, "Cancel Scan" button
- Results table: FILE, SEVERITY, ENGINE MATCH, ACTION columns. Action has [Quarantine] danger button per threat row

**Upload Tab:**
- Drop zone: dashed gold border `#D4AF37`, centered content (upload icon, "Drop files or click to browse", supported formats)
- Hover: `#151515` bg
- Uploaded file list: filename, type, detection result, [Scan] or [Quarantine] button

### 4.3 Defender

- Full-width table: TIME, TYPE, EVENT, THREAT LEVEL
- Filter pills: [All] [Malicious] [Suspicious] [Clean] — active = gold bg + gold text
- Event type badge: small icon + text (File, Reg, Proc, Net)
- Clicking expands row: full path, parent process chain, detection engine details
- Auto-scroll to bottom on new events (optional toggle)

### 4.4 Privacy Guard

- Top bar: [ Enable All ] [ Disable All ] [ "2 changes made" indicator ] [ Apply Changes ]
- Category sections (Telemetry, Cloud, Advertising, Network, etc.):
  - Upper header: uppercase `#666666` Bold, 10px, letter-spacing 1px
  - Thin `#2A2A2A` divider
  - Toggle rows: Name (13px), Description (11px `#666666`, hidden by default — hover tooltip), Toggle (12px `#666666` on `#1A1A1A` bg
- "What this breaks" shown as tooltip, not inline text (reduces noise 70%)
- "Changes made" shows count of toggles that differ from current state (gold dim text)

### 4.5 Quarantine

- Table: DATE, FILE, ORIGINAL PATH, THREAT LEVEL, ACTION
- Empty state: "No quarantined files — Argus is keeping you safe" with green diamond
- Row selection: gold left border, slightly brighter bg
- Detail panel below selected row: file hash (SHA-256), detection reason, timestamp
- Buttons: [ Release Selected ] (gold outline) + [ Delete Permanently ] (danger red)

### 4.6 Settings

- Header tabs: DNS | API Keys | Auto-Update (Linear-style tab bar: gold text + gold underline on active)
- DNS section: Toggle switch (On/Off), profile selector dropdown, Primary/Secondary IP display (monospace font), [ Apply DNS Settings ] gold primary
- API Keys section: Table with Service, Key (masked `•••••••••`), Status (Connected/Not connected). "Add key" button for unconfigured services. UAC elevation required to view/unmask keys.
- Auto-Update section: Policy options — Minor (Silent), Major (Notify), Critical (Notify immediately). Radio-button style selection (only one per category).

---

## 5. File Organization

```
src/Argus.GUI/
├── App.xaml                          ← Merges theme dictionaries
├── MainWindow.xaml                   ← Window frame only (~80 lines)
├── MainWindow.xaml.cs
│
├── Themes/
│   ├── ArgusTheme.xaml               ← All color brushes (foundation)
│   ├── Controls.xaml                  ← Button, ToggleButton, ComboBox styles
│   └── Cards.xaml                     ← Card, Badge, StatusPill styles
│
├── Views/
│   ├── DashboardView.xaml             ← Dashboard (extracted from MainWindow)
│   ├── ScannerView.xaml               ← NEW: Scanner deep scan + upload
│   ├── DefenderView.xaml              ← NEW: Activity log
│   ├── PrivacyGuardView.xaml          ← Renamed from PrivacyGuardTab.xaml
│   ├── QuarantineView.xaml            ← NEW: Quarantine management
│   ├── SettingsView.xaml              ← Extracted from MainWindow
│   └── * (corresponding .xaml.cs files)
│
├── ViewModels/                        ← Unchanged (all bindings preserved)
└── Converters/                        ← Unchanged
```

### Key Architectural Decisions

1. **MainWindow.xaml shrinks to ~80 lines** — only safe Mode banner, sidebar, status bar, `ContentControl`
2. **Each View in its own .xaml file** via `ContentControl.DataTemplate` routing
3. **All existing bindings preserved** — `CurrentView` is set by commands in the existing `MainViewModel`
4. **Theme in separate ResourceDictionary** — one file controls all colors. Future theme tweaks = one file change
5. **No ViewModel changes** — 100% XAML/styling exercise, zero C# logic modified

### Resource Dictionary Merge Order (App.xaml)

1. Themes/ArgusTheme.xaml (colors/brushes
2. Themes/Controls.xaml (component styles reference theme brushes)
3. Themes/Cards.xaml (card/badge styles reference theme brushes)

Later merge over overrides earlier ones — theme must be last for control styles to use its brushes.

---

## 6. Animation & Micro-interactions

| Element | Animation | Duration | Easing |
|---------|-----------|----------|--------|
| Nav active indicator | Gold border slides between items | 150ms | EaseOut |
| Toggle switch thumb | Slide + color transition | 200ms | EaseInOut |
| Progress bar | Deterministic fill (real progress only) | Natural | Linear |
| Card hover border | `#2A2A2A` → `#3A3A3A` | 100ms | Linear |
| Nav hover | Background fade, text brighten | 80 | Linear |
| Safe Mode banner | Fade in from top | 300ms | EaseOut |
| Page transitions | None (instant switch) | Fast feedback |

**What NOT to animate:** Page slide/fade, card entrance, pulsing/glowing on normal elements, sound effects.

**Principle:** Animation reinforces state changes only, never decorates. Linear's approach: UI should feel "calm."

---

## 7. Implementation Plan

### Files to Create

| File | Description |
|------|-------------|
| `Themes/ArgusTheme.xaml` | Color ResourceDictionary |
| `Themes/Controls.xaml` | Button, ToggleButton, ComboBox, TextBox, ScrollViewer styles |
| `Themes/Cards.xaml` | Card, Badge, StatusPill styles |
| `Views/DashboardView.xaml` | Dashboard layout with new design |
| `Views/ScannerView.xaml` | Scanner tab interface |
| `Views/DefenderView.xaml` | Activity log table |
| `Views/PrivacyGuardView.xaml` | Privacy Guard with new toggle styles |
| `Views/QuarantineView.xaml` | Quarantine management table |
| `Views/SettingsView.xaml` | Settings with tabs |

### Files to Modify

| File | Description |
|------|-------------|
| `App.xaml` | Merge theme dictionaries |
| `MainWindow.xaml` | Shrink to frame + sidebar, update styles references |
| `Views/PrivacyGuardTab.xaml` | Update or replace with new designs |
| `MainWindow.xaml.cs` | Only if nav command signatures change |

### Files Unchanged

- All `ViewModels/*.cs`
- All `Converters/*.cs`
- All `.csproj` files
- `App.xaml.cs`

---

## 8. Verification

### Build

`Argus.slnx` — must compile clean

### Visual Verification

- Each screen matches the design spec layout
- Gold accents present on all active states
- Black backgrounds consistent
- Text readable at all contrast ratios (WCAG AA 4.5:1)
- No blue colors remain in the UI
- Safe Mode banner renders correctly (red, no emoji)

### Functional Verification

- Each nav button switches view correctly
- All buttons work (scan, apply, release, delete)
- All toggles respond, privacy toggles apply on IPC
- Safe Mode state triggers banner
- Status bar shows pipe status

### Verification commands

```bash
dotnet build Argus.slnx
dotnet publish src\Argus.GUI -c Release -r win-x64 --self-contained false
```

Then run `Argus.GUI.exe` and screenshot each screen for comparison.

---

## 9. Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| WPF ResourceDictionary merge conflicts | Low | Medium | Test merge order incrementally |
| Custom control templates break existing bindings | Low | High | Test each template before moving to next |
| ScrollViewer styling affects all dialogs | Medium | Low | Scope styles to `Key`-based resources, not implicit where appropriate |
| Navigation indicator animation not smooth | Low | Low | Use `TranslateTransform` on a `Rectangle` element with `Storyboard` — WPF handles this well |
| Theme change causes performance issues | Very Low | High | ResourceDictionary merging is efficient in WPF. No runtime evaluation of colors |

**Overall risk: LOW.** The redesign is purely visual — no C# code changes, all existing bindings preserved. The risk is primarily in WPF XAML styling, which can be iterated quickly.
