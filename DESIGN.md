# Flow Design System

This file defines the active visual system for the Flow launcher (Windows desktop). If implementation differs from this spec, this spec wins.

---

## Design Direction

- **Premium minimal, Nothing-inspired**: restrained, high-contrast, calm.
- **Monochrome-first** interface with selective semantic accents (blue for active states, green/orange/red for semantic meaning).
- **Flat surfaces and strokes only**; no ornamental shadows or gradients.
- **Functional hierarchy** through spacing, weight, tone, and layout (not decoration).
- **Swiss-inspired typography**: tight tracking, clear hierarchy, DM Sans for UI, DM Mono for numbers/stats/results metadata.
- **Floating window architecture**: a single centered popup window with no title bar. No sidebar. No persistent chrome. The launcher appears, the user acts, it disappears.

---

## Architecture

### Window Shell

```
┌─────────────────────────────────────────────┐
│  Search Bar (56px)                          │
│  [ 🔍  Type anything...          ⌘Space ]  │
├─────────────────────────────────────────────┤
│  Results List (max 8 visible rows)          │
│  ┌─────────────────────────────────────┐   │
│  │ [icon]  Result Name    subtitle  ⌘↵ │   │
│  │ [icon]  Result Name    subtitle  ↵  │   │
│  │ [icon]  Result Name    subtitle      │   │
│  │ ...                                  │   │
│  └─────────────────────────────────────┘   │
├─────────────────────────────────────────────┤
│  Detail Panel (conditional, 240px)          │
│  Shown when result selected or extension    │
│  is active (AI, Color, Clipboard, etc.)     │
└─────────────────────────────────────────────┘
```

**Key behaviors:**
- Window is always centered on screen. Fixed width: 680px. Height: dynamic based on results count, max ~560px.
- No title bar. No window controls. No resize handles.
- `Alt+Space` opens the window. `Escape` or click-outside closes it instantly.
- Window appears with opacity 0→1 + scale 0.97→1, 150ms `--ease-out`.
- Window dismisses with opacity 1→0 + scale 1→0.97, 100ms `--ease-in-out`.
- Dark mode toggles via `data-theme="dark"` on `<html>`, driven by store state.
- When an extension is active (AI, Clipboard, Color, Timer, IP), the detail panel expands below the results list.
- Settings screen replaces the results area entirely — triggered by typing `settings`.

### Shared State

- Zustand store (`useFlowStore`) is the single source of truth for query, results, selected index, active extension, AI state, config, and usage frequency.
- Config is persisted to a local JSON file via Tauri fs API.

---

## Tokens

### Color — Light Mode

| Role | CSS Variable | Value |
|------|-------------|-------|
| Page background | `--bg` | `#fafafa` |
| Surface | `--surface` | `#ffffff` |
| Surface low | `--surface-low` | `#f5f5f5` |
| Surface container | `--surface-container` | `#dcdcdc` |
| Surface container high | `--surface-container-high` | `#c2c2c2` |
| Primary text (ink) | `--ink` | `#000000` |
| Secondary text (muted) | `--muted` | `#757575` |
| Tertiary text (faint) | `--faint` | `#9a9a9a` |
| Border (hairline) | `--hairline` | `rgba(0,0,0,0.08)` |
| Border (line) | `--line` | `#dcdcdc` |
| Border strong | `--line-strong` | `#a8a8a8` |
| Primary action (accent) | `--accent` | `#000000` |
| Accent strong | `--accent-strong` | `#1a1a1a` |

### Color — Dark Mode

| Role | CSS Variable | Value |
|------|-------------|-------|
| Page background | `--bg` | `#0e0e0e` |
| Surface | `--surface` | `#161616` |
| Surface low | `--surface-low` | `#1a1a1a` |
| Surface container | `--surface-container` | `#202020` |
| Surface container high | `--surface-container-high` | `#282828` |
| Primary text (ink) | `--ink` | `#f2f2f2` |
| Secondary text (muted) | `--muted` | `#9a9a9a` |
| Tertiary text (faint) | `--faint` | `#747474` |
| Border (line) | `--line` | `#2c2c2c` |
| Border strong | `--line-strong` | `#404040` |
| Primary action (accent) | `--accent` | `#f2f2f2` |
| Accent strong | `--accent-strong` | `#d8dde3` |

### Active / Selected State Colors

Used for the selected result row and active extension indicators.

| Role | CSS Variable | Light | Dark |
|------|-------------|-------|------|
| Selected row background | `--selected-bg` | `#000000` | `#f2f2f2` |
| Selected row text | `--selected-text` | `#ffffff` | `#000000` |
| Selected row muted | `--selected-muted` | `rgba(255,255,255,0.6)` | `rgba(0,0,0,0.5)` |
| Hover row background | `--hover-bg` | `#f5f5f5` | `#1a1a1a` |
| Active extension bg | `--ext-bg` | `#f5f5f5` | `#1a1a1a` |

### Semantic Colors

| Token | Light | Dark |
|-------|-------|------|
| Success / green | `#1a7a40` | `#5ee577` |
| Green wash | `#d4f0e0` | `#0d2a1a` |
| Warning / orange | `#a04f00` | `#ffc779` |
| Orange wash | `#fde8cc` | `#3a2611` |
| Error / red | `#c0180f` | `#ff7b7b` |
| Red wash | `#fde4e2` | `#3a1214` |
| Blue (AI, active) | `#3650d4` | `#a0b4ff` |
| Blue wash | `#e8edfb` | `#171f42` |

### Result Type Accent Colors

Each result type has a subtle left accent rail (2px) on the selected row:

| Result Type | Color |
|-------------|-------|
| App | `#000000` (light) / `#f2f2f2` (dark) — same as ink |
| File | `#3650d4` (blue) |
| AI response | `#3650d4` (blue) |
| Calculator | `#1a7a40` (green) |
| Clipboard | `#a04f00` (orange) |
| Color picker | dynamic — matches the picked color |
| Timer | `#a04f00` (orange) |
| Settings | `#000000` / `#f2f2f2` |

### Typography

| Usage | Font | Size | Weight | Letter-spacing |
|-------|------|------|--------|----------------|
| Search bar input | DM Sans | 17px | 400 | `--ls-normal` |
| Search bar placeholder | DM Sans | 17px | 400 | `--ls-normal` |
| Result name (primary) | DM Sans | 14px | 500 | `--ls-normal` |
| Result subtitle (secondary) | DM Sans | 12px | 400 | `--ls-normal` |
| Keyboard shortcut hint | DM Mono | 11px | 500 | `--ls-wide` |
| Section label / eyebrow | DM Sans | 11px | 700 | `--ls-widest` (uppercase) |
| Extension output (large) | DM Mono | 28px | 700 | `--ls-tighter` |
| Extension output (body) | DM Sans | 14px | 400 | `--ls-normal` |
| Settings label | DM Sans | 14px | 500 | `--ls-normal` |
| Settings value | DM Sans | 14px | 400 | `--ls-normal` |
| AI response text | DM Sans | 14px | 400 | `--ls-normal` |
| Footer / version | DM Sans | 11px | 400 | `--ls-normal` |

### Letter-Spacing Tokens

| Token | Value | Usage |
|-------|-------|-------|
| `--ls-tightest` | `-0.06em` | Large extension output numbers |
| `--ls-tighter` | `-0.04em` | Extension display values |
| `--ls-tight` | `-0.025em` | Result names |
| `--ls-normal` | `-0.01em` | Body text |
| `--ls-wide` | `0.05em` | Monospace keyboard hints |
| `--ls-wider` | `0.08em` | Label text |
| `--ls-widest` | `0.12em` | Eyebrows, section labels |

### Spacing Scale

| Token | Value |
|-------|-------|
| `--space-1` | 4px |
| `--space-2` | 8px |
| `--space-3` | 12px |
| `--space-4` | 16px |
| `--space-5` | 24px |
| `--space-6` | 32px |
| `--space-7` | 48px |
| `--space-8` | 64px |
| `--space-9` | 80px |
| `--space-10` | 96px |

### Border Radius

| Token | Value | Usage |
|-------|-------|-------|
| `--radius-sm` | 4px | Keyboard shortcut badges, small tags |
| `--radius-md` | 6px | — |
| `--radius-lg` | 8px | Extension action buttons |
| `--radius-xl` / `--sys-radius` | 10px | Search bar, result rows, settings rows, cards |
| Pill (`9999px`) | — | Extension pills, badges |
| Window | 12px | The main window outer radius |

### Elevation

- **No drop shadows** for internal UI. Depth is communicated through tone layering (`--surface` vs `--surface-container`) and border contrast.
- **Window shadow only**: `0 32px 80px rgba(0,0,0,0.45), 0 8px 24px rgba(0,0,0,0.25)` — the window floats above the desktop. This is the only shadow in the entire app.
- Detail panel uses `border-top: 1px solid var(--line)` to separate from results list — no shadow.

### Transitions

| Token | Value |
|-------|-------|
| `--ease-out` | `cubic-bezier(0.22, 1, 0.36, 1)` |
| `--ease-in-out` | `cubic-bezier(0.4, 0, 0.2, 1)` |
| `--duration-fast` | 150ms |
| `--duration-normal` | 200ms |
| `--duration-slow` | 300ms |

---

## Component Rules

### Search Bar

- Height: 56px. Full width of window.
- Background: `--bg`. No border. No radius on its own — the window radius clips it.
- Left: search icon (18px, Lucide, strokeWidth 1.5, `--muted`). Padding-left: 20px.
- Input: DM Sans 17px, `--ink`. Placeholder: `--muted`. No outline on focus — the bar is always active.
- Right: mode indicator (e.g. "AI" badge when AI mode active) + `⌥Space` hint in DM Mono 11px `--faint`.
- Border-bottom: `1px solid var(--line)` separating it from results.
- The search bar is always focused when the window is open. Never loses focus.

### Result Row

- Height: 52px. Full width. Padding: 0 16px.
- Layout: `display: flex`, `align-items: center`, `gap: 12px`.
- Left: icon (20px square). App icons are real .ico extracted icons. File icons are Lucide file-type icons. Extension icons are Lucide icons in `--muted`.
- Center: result name (DM Sans 14px 500 `--ink`) + subtitle below (DM Sans 12px `--muted`). `flex: 1`.
- Right: keyboard shortcut badge (DM Mono 11px, `--surface-container` bg, `--muted` text, `--radius-sm`, px-6 py-2).
- Default: `background: transparent`.
- Hover: `background: var(--hover-bg)`, transition `--duration-fast`.
- Selected: `background: var(--selected-bg)`, name color `var(--selected-text)`, subtitle `var(--selected-muted)`, shortcut badge inverts.
- Selected also shows a 2px left accent rail (result-type color) — `border-left: 2px solid [accent]`, padding-left adjusted.
- No border between rows. Separation is spacing only.
- `border-radius: var(--sys-radius)` on selected row only (via outline or bg clip).

### Section Label

- 11px, DM Sans 700, uppercase, `--ls-widest`, `--muted` color.
- Padding: `var(--space-3)` top, `var(--space-2)` bottom, 16px horizontal.
- Used to group results: "APPLICATIONS", "FILES", "EXTENSIONS".
- No background. No border. No decoration.

### Detail Panel

- Sits below the results list. `border-top: 1px solid var(--line)`.
- Background: `--surface-low`.
- Padding: 20px.
- Content varies by extension — see Extension Components.
- Enters with opacity 0→1 + translateY 8→0, `--duration-normal` `--ease-out`.
- Exits with opacity 1→0, `--duration-fast`.

### Extension: Calculator

- Detail panel shows result in DM Mono 36px 700 `--ink`, letter-spacing `--ls-tightest`.
- Expression shown above in DM Mono 13px `--muted`.
- Copy hint: "↵ to copy" in 11px DM Sans `--faint` bottom-right.

### Extension: AI Assistant

- Detail panel shows streaming response in DM Sans 14px `--ink`, line-height 1.6.
- While streaming: animated cursor (blinking `|`) at end of text.
- "Powered by Groq" in 11px DM Sans `--faint` bottom-right after completion.
- Error state: red wash background, error message in `--error` color.
- "No internet" state: orange wash, "AI unavailable — check your connection" message.

### Extension: Clipboard

- Detail panel shows scrollable list of last 20 clipboard items.
- Each item: single line, DM Sans 13px `--ink`, truncated with ellipsis. 40px height.
- Selected item: `--selected-bg` + `--selected-text`, same as result row.
- Scroll: native, no custom scrollbar styling.

### Extension: Color Picker

- Detail panel shows: large color swatch (80px × 80px, `border-radius: 8px`, `border: 1px solid var(--line)`).
- Right of swatch: Hex value (DM Mono 20px 700), RGB row (DM Mono 13px `--muted`), HSL row (DM Mono 13px `--muted`).
- "↵ to copy hex" hint in 11px DM Sans `--faint`.

### Extension: Timer

- Detail panel shows countdown in DM Mono 48px 700 `--ink`, `--ls-tightest`.
- Below: progress bar (6px height, `border-radius: 3px`, `--surface-container` bg, `--ink` fill).
- "Cancel" ghost button bottom-right.

### Extension: IP Address

- Detail panel shows two rows: "LOCAL" label + IP in DM Mono 16px 600, "PUBLIC" label + IP in DM Mono 16px 600.
- Labels: 11px DM Sans 700 uppercase `--ls-widest` `--muted`.
- "↵ to copy" hint per row on hover.

### Extension: Settings

- Full replacement of results area. No results list visible.
- Settings layout: two sections (Appearance, AI).
- Each setting row: 48px height, label left (DM Sans 14px 500 `--ink`), control right.
- Controls: segmented control, toggle switch, or text input — same rules as below.
- `border-bottom: 1px solid var(--line)` between rows. Last row no border.

### Buttons

- **Primary**: `--accent` fill, contrasting text (`#fff` light / `--bg` dark). `border-radius: var(--sys-radius)`. No shadow. Hover shifts to `--accent-strong`.
- **Ghost**: transparent background, `border: 1px solid var(--line)`. `--muted` text, `--ink` on hover.
- **Icon buttons**: 36px square, ghost style, `border-radius: var(--sys-radius)`.
- **Focus ring**: `2px solid var(--accent)`, `outline-offset: 2px`.
- **Disabled**: `opacity: 0.35`, `cursor: not-allowed`.

### Segmented Control

- Container: `display: flex`, `padding: 3px`, `border: 1px solid var(--line)`, `border-radius: var(--radius-sm)`, `background: var(--surface-container)`.
- Active segment: `background: var(--surface)`, `color: var(--ink)`.
- Inactive: transparent, `color: var(--muted)`.
- Used in: settings (theme, font size, radius style).

### Toggle Switch

- Track: 44px × 24px, `border-radius: 99px`, `background: var(--line-strong)`.
- Knob: 18px × 18px, white, `border-radius: 50%`. Animates left/right 150ms `--ease-out`.
- Active (`.on`): `background: var(--accent)`.

### Inputs (Settings)

- Border: `1px solid var(--line)`.
- Border-radius: `var(--sys-radius)`.
- Placeholder: `--muted`.
- Focus: `outline: 2px solid var(--accent)`, `outline-offset: 2px`. No glow.
- Height: 40px. Padding: 0 12px.

---

## Screen Layouts

### Main Window (search + results)

```
┌────────────────────────────────────────────┐  ← border-radius: 12px
│ 🔍  Type anything...            ⌥Space    │  ← SearchBar, 56px
├────────────────────────────────────────────┤  ← 1px --line
│ APPLICATIONS                               │  ← Section label, 32px
│ [icon]  Figma              Design   ⌘↵    │  ← ResultRow, 52px, selected
│ [icon]  Firefox            Browser  ↵     │  ← ResultRow, 52px
│ [icon]  VS Code            Code           │
├────────────────────────────────────────────┤  ← 1px --line
│ FILES                                      │  ← Section label
│ [icon]  design.md          ~/Projects     │
│ [icon]  ARCHITECTURE.md    ~/Projects     │
├────────────────────────────────────────────┤  ← 1px --line (if extension active)
│  Detail Panel (extension output)           │  ← 160px min, grows with content
└────────────────────────────────────────────┘
```

**Empty state (no query):**
- No results list visible.
- Center of results area: "Start typing to search apps and files." in DM Sans 14px `--muted`, centered.
- Below: recent apps (last 5 opened) shown as a horizontal row of icon + name pills.

**No results state:**
- Single row: search icon + "No results for '[query]'" in DM Sans 14px `--muted`.
- Below: "Try searching for something else or press ↵ to ask AI." in 12px `--faint`.

**Loading state (file search debounce):**
- Three animated dots in `--muted` color, centered in results area. No spinner.

### Settings Screen (triggered by `settings` query)

```
┌────────────────────────────────────────────┐
│ 🔍  settings                    ⌥Space    │
├────────────────────────────────────────────┤
│ APPEARANCE                                 │
│ Theme               [ Light │ Dark │ Sys ] │
│ Font Size           [ Compact│Comf │Large] │
│ Border Radius       [ Sharp │Round│ Pill ] │
│ Window Opacity      [━━━━━━━●━━━━━] 90%   │
├────────────────────────────────────────────┤
│ AI                                         │
│ Groq API Key        [________________]     │
│ AI Model            llama3-8b-8192         │
├────────────────────────────────────────────┤
│ SHORTCUTS                                  │
│ Open Flow           Alt+Space              │
├────────────────────────────────────────────┤
│ DATA                                       │
│ Clear usage history              [Clear]   │
├────────────────────────────────────────────┤
│                      Flow v0.1.0           │
└────────────────────────────────────────────┘
```

---

## Window Behavior Rules

- **Always on top**: `alwaysOnTop: true` in Tauri config.
- **No taskbar entry**: `skipTaskbar: true`.
- **No title bar**: `decorations: false`.
- **Transparent background**: `transparent: true` — allows the window shadow and border-radius to render correctly.
- **Focus on open**: window steals focus immediately when opened.
- **Focus lost**: window closes instantly when focus moves to another app. Same behavior as Spotlight.
- **Escape**: closes the window. If in a sub-state (extension active), first press resets to main search. Second press closes.
- **Click outside**: closes the window instantly.

---

## Keyboard Navigation Rules

| Key | Action |
|-----|--------|
| `↓` / `Tab` | Move selection down |
| `↑` / `Shift+Tab` | Move selection up |
| `Enter` | Open / execute selected result |
| `Ctrl+Enter` | Secondary action (open file location, copy value) |
| `Escape` | Reset query or close window |
| `Alt+Space` | Open / close window (global) |
| Any alphanumeric | Types into search bar (bar is always focused) |
| `Backspace` | Clears last character in query |

---

## Animation Rules

✅ Allowed:
- Window open: opacity 0→1 + scale 0.97→1, 150ms `--ease-out`
- Window close: opacity 1→0 + scale 1→0.97, 100ms `--ease-in-out`
- Row hover: backgroundColor, `--duration-fast`
- Row selection shift: backgroundColor, `--duration-fast`
- Detail panel enter: opacity 0→1 + translateY 8→0, `--duration-normal` `--ease-out`
- Detail panel exit: opacity 1→0, `--duration-fast`
- AI streaming: text appears token by token — streaming IS the animation
- Timer progress bar: width transition, 1s linear per second
- Toggle switch knob: translateX, `--duration-fast` `--ease-out`

❌ Not allowed:
- No stagger animations on result rows — speed is the priority
- No spring physics — this is a utility tool, not a consumer app
- No layout shift animations (no animating height/width of the window)
- No loading spinners — three-dot pulse only if needed
- No celebration animations
- No slide transitions between extensions — cut instantly
- No scroll animations — instant jump to selected row

**Default rule: if in doubt, do not animate it.**

---

## Implementation Notes

- All CSS custom properties defined in `src/styles/global.css`. Dark mode via `[data-theme="dark"]` selector.
- Component styles use CSS Modules. No inline styles. No Tailwind.
- Token variables only for colors, spacing, radius. No hardcoded values in component files.
- Window background is `--bg`. The Tauri window is `transparent: true` — the `--bg` color of the root div IS the window background, clipped by the 12px border-radius.
- The outer window border: `1px solid var(--line-strong)` applied to the root `#app` div, `border-radius: 12px`.
- The single window shadow is applied to `#app` via `box-shadow` — not to Tauri, not to any inner element.
- Font loading: DM Sans and DM Mono loaded via `@font-face` from local assets in `src/assets/fonts/`. Never from Google CDN — the launcher must work offline.