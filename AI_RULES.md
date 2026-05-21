# AI_RULES.md — Flow

## Tech Stack — Locked

| Layer | Technology | Notes |
|-------|-----------|-------|
| Desktop framework | Tauri 2.x | Only. Not Electron. Not NW.js. |
| Frontend | React 19 + TypeScript | Only. Not Vue. Not Svelte. |
| Build tool | Vite 6 | Only. Not Webpack. Not Parcel. |
| Backend language | Rust (via Tauri) | Only for system-level commands |
| State management | Zustand | Only. Not Redux. Not Context for global state. |
| Search ranking | Fuse.js | Only. Do not build a custom fuzzy search algorithm. |
| AI API | Groq API (`llama3-8b-8192`) | Only. Not OpenAI. Not Anthropic. Not local models. |
| Fonts | Inter (UI) | Loaded via CSS @font-face from local assets |
| Icons (UI) | Lucide React | Only. Not Heroicons. Not FontAwesome. |
| Styling | CSS Modules + CSS variables | Only. Not Tailwind. Not styled-components. Not inline styles. |
| Config persistence | JSON file via Tauri fs API | Only. Not localStorage. Not SQLite. Not Supabase. |
| Frequency tracking | JSON file via Tauri fs API | Only. Same config directory. |

**Do not install packages outside this stack without explicit approval.**

---

## Design System — Locked

All design tokens are defined in `src/constants/tokens.ts` and applied as CSS variables in `src/styles/global.css`. Never hardcode a color, spacing value, or font size outside of these files.

### Colors

```typescript
// src/constants/tokens.ts

export const COLORS = {
  // Flow Dark (default theme)
  dark: {
    bgPage:       '#0E0F18',   // Main window background
    bgSurface:    '#161724',   // Results list background
    bgElevated:   '#1E1F2E',   // Detail panel, settings panel
    bgPressed:    '#242538',   // Hovered/selected result row
    bgInput:      '#161724',   // Search bar background
    textPrimary:  '#EEF0FF',   // App names, file names, main labels
    textSecondary:'#8B96B0',   // Subtitles, file paths, metadata
    textMuted:    '#3D4A60',   // Placeholder text, disabled states
    border:       '#1E2030',   // Hairline borders
    borderStrong: '#2A2D45',   // Panel dividers
  },
  // Flow Light theme
  light: {
    bgPage:       '#F8F7F5',
    bgSurface:    '#FFFFFF',
    bgElevated:   '#FFFFFF',
    bgPressed:    '#F2F1EF',
    bgInput:      '#FFFFFF',
    textPrimary:  '#1A1A2E',
    textSecondary:'#6B7280',
    textMuted:    '#B8BCC8',
    border:       '#EBEBEB',
    borderStrong: '#D8D9E0',
  },
  // Semantic colors (same in all themes)
  semantic: {
    accent:         '#5B6AF0',   // Default accent — indigo violet
    accentSoft:     '#1E2040',   // Accent at 12% — dark mode
    accentSoftLight:'#EEF0FD',   // Accent at 10% — light mode
    success:        '#22C55E',
    successSoft:    '#052E16',
    warning:        '#F59E0B',
    warningSoft:    '#451A03',
    destructive:    '#EF4444',
    destructiveSoft:'#2D0A0A',
  }
} as const
```

### Fonts

```typescript
export const FONTS = {
  family: {
    ui:   '"Inter", system-ui, sans-serif',   // All UI text
    mono: '"JetBrains Mono", monospace',       // File paths, code, IP addresses, calc results
  },
  size: {
    xs:   '11px',   // Keyboard shortcut hints, badges
    sm:   '12px',   // Subtitles, metadata, file paths
    base: '14px',   // Default UI text, result names (compact mode)
    md:   '15px',   // Default UI text (comfortable mode)
    lg:   '17px',   // Default UI text (large mode)
    xl:   '20px',   // Search bar input text
    xxl:  '28px',   // AI response, extension output
  },
  weight: {
    regular: 400,
    medium:  500,
    semibold:600,
    bold:    700,
  }
} as const
```

### Spacing

```typescript
export const SPACING = {
  '1':  '4px',
  '2':  '8px',
  '3':  '12px',
  '4':  '16px',
  '5':  '20px',
  '6':  '24px',
  '8':  '32px',
  '10': '40px',
  '12': '48px',
} as const

// Window dimensions — fixed, never change
export const WINDOW = {
  width:            680,    // px — matches tauri.conf.json
  searchBarHeight:  56,     // px
  resultItemHeight: 52,     // px
  maxResultsShown:  8,      // Before scroll
  detailPanelWidth: 280,    // px — shown when window expands
} as const
```

### Border Radius

```typescript
export const RADIUS = {
  sharp:   { window: '8px',  item: '4px',  button: '4px'  },
  rounded: { window: '12px', item: '8px',  button: '8px'  },
  pill:    { window: '20px', item: '12px', button: '12px' },
} as const
// Default: 'rounded'
// Applied via CSS variable --flow-radius-window, --flow-radius-item, --flow-radius-button
```

### Shadows

```typescript
export const SHADOWS = {
  window: '0 24px 64px rgba(0,0,0,0.6), 0 8px 24px rgba(0,0,0,0.4)',
  // Only one shadow in the entire app — the main window shadow
  // Nothing else has a shadow
} as const
```

---

## Animation Rules

✅ Allowed:
- Window appear: opacity 0→1 + scale 0.97→1, 150ms easeOutCubic
- Window dismiss: opacity 1→0 + scale 1→0.97, 100ms easeInCubic
- Result selection highlight: backgroundColor transition, 80ms easeOut
- Detail panel slide in: translateX 20→0 + opacity 0→1, 200ms easeOutCubic
- Extension view swap: opacity 0→1, 150ms easeOut
- AI response: text streams in — no animation needed, streaming IS the animation
- Settings panel: opacity 0→1 + translateY 8→0, 200ms easeOutCubic
- Result item hover: backgroundColor 80ms easeOut

❌ Not allowed:
- No stagger animations on result items — speed is the priority, stagger adds perceived lag
- No spring physics — this is a utility tool, not a consumer app
- No layout animations — no animating height or width changes
- No loading spinners — use pulsing dots (3 dots) if needed
- No page transition animations between extensions — cut, don't animate
- No scroll animations — instant scroll to selected item
- No celebration animations

**Default rule: if in doubt, do not animate it.**

---

## Reusable Components — Required

These components must be extracted and reused. Do not duplicate their logic:

| Component | Used In | Rule |
|-----------|---------|------|
| `ResultItem.tsx` | `ResultsList`, every extension that returns results | Single source of truth for result row appearance |
| `SearchBar.tsx` | `App.tsx` only — never duplicated | Single input, single state owner |
| `DetailPanel.tsx` | `App.tsx` only — receives content as children | Never embed detail content directly in App |
| `ExtensionView.tsx` | `App.tsx` — renders the active extension | All extensions render through this wrapper |
| `SettingsPanel.tsx` | Activated when query === 'settings' | Full settings screen, self-contained |

---

## Code Quality — Non-Negotiables

### TypeScript
- `strict: true` in `tsconfig.json` — no exceptions
- No `any` types anywhere — use `unknown` and narrow it
- Every Tauri command has a typed wrapper in `src/lib/tauri.ts`
- All Zustand store actions are typed in the store interface

### Components
- Maximum 200 lines per component file — split if larger
- Functional components only — no class components
- Every component handles all three states: loading, error, empty
- No component fetches data directly — all data fetching in hooks
- Props interfaces defined above the component, never inline

### Hooks
- All Tauri calls happen inside hooks — never call `invoke` directly in a component
- Every hook returns: `{ data, isLoading, error }` shape minimum
- Hooks are the only place side effects live

### Styling
- No inline styles anywhere — zero exceptions
- All colors from CSS variables (`--flow-accent`, `--flow-bg-page`, etc.)
- All spacing from the spacing scale — no arbitrary pixel values
- CSS Modules scoped per component — no global class names except in `global.css`

### Performance
- Search results must render in under 50ms — profile if slower
- App list is fetched once on startup and cached — never re-fetched during a session
- File search is debounced 150ms — never fires on every keystroke
- Result list is virtualized if more than 20 items — use `react-window`

---

## Security — Non-Negotiables

### API Key Protection
- `FLOW_GROQ_API_KEY` lives in `.env` only — never in source code
- The Groq API call is made exclusively from `src-tauri/src/commands/ai.rs`
- React frontend never sees the API key — it calls a Tauri command, not the API directly
- The config file stores the user-entered API key — it is stored with Tauri's secure storage, not plain JSON

### Data Ownership
- All user data (config, usage frequency, clipboard history) lives on the user's machine only
- No telemetry. No analytics. No crash reporting. No external data transmission except the Groq API call.
- Clipboard history is stored in memory only — never written to disk

### Input Validation
- All search queries are sanitized before being passed to Tauri commands
- Math evaluator (`src/lib/math.ts`) uses a safe parser — never `eval()`
- File paths returned from Tauri are validated before being passed back to the UI

---

## AI Feature Rules

- Model: `llama3-8b-8192` via Groq — do not change the model without updating this file
- Every AI call is stateless — no conversation history is sent
- The AI Tauri command streams the response — React reads it token by token via Tauri events
- If the API key is not set: show "Add your Groq API key in settings" — never crash
- If the API call fails: show the error message in the detail panel — never show a blank panel
- If offline: detect before calling, show "AI unavailable — no internet connection"
- Maximum query length: 2000 characters — truncate and warn if exceeded
- AI responses are plain text only — no markdown rendering in V1

---

## Extension Rules

- Each extension is a self-contained React component in `src/extensions/`
- Extensions must not import from each other
- Extension trigger detection lives in `src/lib/extensions.ts` — not in components
- Every extension handles its own loading and error state
- Extensions cannot make network calls except `AiAssistant.tsx`
- The calculator extension uses `src/lib/math.ts` — never `eval()`
- Timer extension fires an OS notification via Tauri when complete

---

## Git Rules

- One branch per feature: `feature/search-bar`, `feature/ai-extension`, `feature/themes`
- Commit after completing each PLAN.md step — commit message = step name
- Never commit `.env` — it is in `.gitignore` permanently
- Never commit `flow.config.json` — user config is local only
- `PRD.md`, `ARCHITECTURE.md`, `AI_RULES.md`, `PLAN.md` live in project root permanently and are committed
- Main branch only receives merges from completed feature branches — never commit directly to main
