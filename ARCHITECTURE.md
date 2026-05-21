# ARCHITECTURE.md — Flow

## Folder Structure

```
flow/
├── src-tauri/                          # Tauri Rust backend
│   ├── src/
│   │   ├── main.rs                     # Tauri app entry point, window setup
│   │   ├── commands/
│   │   │   ├── mod.rs                  # Command module exports
│   │   │   ├── apps.rs                 # Discover installed apps from Start Menu
│   │   │   ├── files.rs                # File search via Windows Search Index
│   │   │   ├── launcher.rs             # Launch app or open file
│   │   │   ├── clipboard.rs            # Read clipboard history
│   │   │   ├── shortcuts.rs            # Global keyboard shortcut registration
│   │   │   ├── ai.rs                   # Groq API call (never exposes key to frontend)
│   │   │   ├── config.rs               # Read/write local config JSON file
│   │   │   └── system.rs               # IP address, system info
│   │   └── lib.rs
│   ├── tauri.conf.json                 # Tauri window config (frameless, centered, size)
│   ├── Cargo.toml                      # Rust dependencies
│   └── icons/                          # App icons
│
├── src/                                # React frontend
│   ├── main.tsx                        # React entry point
│   ├── App.tsx                         # Root component, router
│   │
│   ├── components/
│   │   ├── SearchBar.tsx               # The main input field at top of window
│   │   ├── ResultsList.tsx             # Virtualized list of search results
│   │   ├── ResultItem.tsx              # Single result row (icon + name + subtitle + shortcut)
│   │   ├── DetailPanel.tsx             # Right-side detail panel
│   │   ├── ExtensionView.tsx           # Renders the active extension output
│   │   └── SettingsPanel.tsx           # Theme and appearance settings screen
│   │
│   ├── extensions/
│   │   ├── index.ts                    # Extension registry — maps trigger → component
│   │   ├── Calculator.tsx              # Math expression evaluator and result display
│   │   ├── Clipboard.tsx               # Clipboard history list
│   │   ├── ColorPicker.tsx             # Color preview and value display
│   │   ├── Timer.tsx                   # Countdown timer with notification
│   │   ├── IpAddress.tsx               # Local and public IP display
│   │   └── AiAssistant.tsx             # Streaming AI response display
│   │
│   ├── hooks/
│   │   ├── useSearch.ts                # Unified search — combines apps + files + extensions
│   │   ├── useApps.ts                  # Fetches and caches installed app list
│   │   ├── useFiles.ts                 # File search with debounce
│   │   ├── useKeyboard.ts              # Arrow key navigation, Enter, Escape handling
│   │   ├── useExtension.ts             # Detects active extension from query string
│   │   ├── useAi.ts                    # Manages AI query state and streaming
│   │   ├── useConfig.ts                # Reads and writes config/settings
│   │   └── useTheme.ts                 # Applies theme tokens to CSS variables
│   │
│   ├── store/
│   │   └── flowStore.ts                # Zustand store — global app state
│   │
│   ├── lib/
│   │   ├── fuzzy.ts                    # Fuse.js setup and search ranking logic
│   │   ├── extensions.ts               # Extension trigger detection logic
│   │   ├── math.ts                     # Safe math expression evaluator
│   │   ├── frequency.ts                # Tracks usage frequency per app/file
│   │   └── tauri.ts                    # Typed wrappers around Tauri invoke calls
│   │
│   ├── types/
│   │   ├── result.ts                   # SearchResult, AppResult, FileResult types
│   │   ├── config.ts                   # Config, Theme, AccentColor types
│   │   └── extension.ts                # Extension, ExtensionTrigger types
│   │
│   ├── constants/
│   │   ├── themes.ts                   # Built-in theme presets (Flow Dark, Nord, etc.)
│   │   ├── extensions.ts               # Extension trigger patterns and metadata
│   │   └── tokens.ts                   # Design tokens (colors, spacing, radius, fonts)
│   │
│   └── styles/
│       ├── global.css                  # CSS variables, base reset, font imports
│       └── themes.css                  # Per-theme CSS variable overrides
│
├── config/
│   └── flow.config.json                # User config file (theme, accent, shortcuts, API key)
│
├── .env                                # Never committed — GROQ_API_KEY lives here
├── .gitignore
├── package.json
├── tsconfig.json
├── vite.config.ts
├── PRD.md                              # Product requirements — lives in root permanently
├── ARCHITECTURE.md                     # This file — lives in root permanently
├── AI_RULES.md                         # Engineering rules — lives in root permanently
└── PLAN.md                             # Build roadmap — lives in root permanently
```

---

## Local State — Zustand Store

```typescript
// store/flowStore.ts

interface FlowStore {
  // Search state
  query: string                         // Current text in search bar
  results: SearchResult[]               // Current search results
  selectedIndex: number                 // Which result is highlighted
  isLoading: boolean                    // True while file search is in progress

  // Window state
  isOpen: boolean                       // Whether the Flow window is visible
  activeExtension: Extension | null     // Currently active built-in extension

  // AI state
  aiQuery: string                       // The current AI question
  aiResponse: string                    // Streamed AI response text
  aiIsStreaming: boolean                // True while response is streaming
  aiError: string | null                // Error message if AI call failed

  // Config state
  config: FlowConfig                    // Full user config loaded from JSON file

  // Frequency tracking
  usageMap: Record<string, number>      // app/file path → open count

  // Actions
  setQuery: (query: string) => void
  setResults: (results: SearchResult[]) => void
  setSelectedIndex: (index: number) => void
  moveSelectionUp: () => void
  moveSelectionDown: () => void
  openSelected: () => void
  dismissWindow: () => void
  setConfig: (config: Partial<FlowConfig>) => void
  incrementUsage: (path: string) => void
  resetSearch: () => void
}
```

---

## Config Schema

```typescript
// types/config.ts

interface FlowConfig {
  theme: 'dark' | 'light' | 'system'
  preset: 'flow-dark' | 'flow-light' | 'dracula' | 'nord' | 'catppuccin'
  accentColor: string                   // Hex value e.g. "#5B6AF0"
  opacity: number                       // 0.7 to 1.0
  borderRadius: 'sharp' | 'rounded' | 'pill'
  fontSize: 'compact' | 'comfortable' | 'large'
  shortcut: string                      // e.g. "Alt+Space"
  showDetailPanel: boolean
  groqApiKey: string                    // Stored encrypted in config file
  maxFileResults: number                // Default 20
  maxAppResults: number                 // Default 10
  maxClipboardItems: number             // Default 20
}
```

---

## Search Result Types

```typescript
// types/result.ts

type ResultType = 'app' | 'file' | 'extension' | 'ai'

interface BaseResult {
  id: string
  type: ResultType
  name: string
  subtitle: string
  icon: string                          // Path to icon or emoji fallback
  score: number                         // Fuse.js relevance score
  frequencyScore: number                // Usage frequency boost
}

interface AppResult extends BaseResult {
  type: 'app'
  executablePath: string
  shortcutPath: string
}

interface FileResult extends BaseResult {
  type: 'file'
  fullPath: string
  extension: string
  lastModified: string
  size: number
}

interface ExtensionResult extends BaseResult {
  type: 'extension'
  extensionId: string
  component: React.FC
}
```

---

## Extension Registry

```typescript
// extensions/index.ts

interface ExtensionDefinition {
  id: string
  name: string
  trigger: RegExp | string              // Pattern that activates this extension
  icon: string
  component: React.FC<{ query: string }>
  description: string
}

const extensions: ExtensionDefinition[] = [
  { id: 'calculator', trigger: /^[\d\s\+\-\*\/\(\)\.]+$/, ... },
  { id: 'clipboard',  trigger: 'clip', ... },
  { id: 'color',      trigger: /^#[0-9a-fA-F]{3,6}$/, ... },
  { id: 'timer',      trigger: /^timer\s+\d+[smh]$/, ... },
  { id: 'ip',         trigger: 'ip', ... },
  { id: 'ai',         trigger: /^ai\s+.+/, ... },
  { id: 'settings',   trigger: 'settings', ... },
]
```

---

## Tauri Window Configuration

```json
// tauri.conf.json (window section)
{
  "windows": [{
    "title": "Flow",
    "width": 680,
    "height": 480,
    "resizable": false,
    "fullscreen": false,
    "decorations": false,
    "transparent": true,
    "alwaysOnTop": true,
    "center": true,
    "visible": false,
    "skipTaskbar": true,
    "focused": true
  }]
}
```

---

## Naming Conventions

| Category | Convention | Example |
|----------|-----------|---------|
| Folders | lowercase with hyphens | `src/extensions/` |
| Components | PascalCase | `ResultItem.tsx` |
| Hooks | camelCase with `use` prefix | `useSearch.ts` |
| Store | camelCase with `Store` suffix | `flowStore.ts` |
| Types/Interfaces | PascalCase | `SearchResult`, `FlowConfig` |
| Constants | SCREAMING_SNAKE_CASE | `MAX_RESULTS` |
| CSS variables | `--flow-` prefix | `--flow-accent`, `--flow-bg` |
| Tauri commands | snake_case | `search_files`, `launch_app` |
| Config keys | camelCase | `accentColor`, `borderRadius` |
| Env variables | `FLOW_` prefix | `FLOW_GROQ_API_KEY` |
