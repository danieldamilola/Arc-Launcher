# PRD.md — Flow

## What Is Flow?

Flow is a keyboard-first app launcher for Windows. Press a global shortcut, type anything, and instantly search installed apps, files, and built-in tools — all from a single floating window. Flow replaces the need to click through the Start Menu or File Explorer. It is fast, minimal, themeable, and has an AI assistant built in.

---

## Who Is It For?

| Role | What They Can Do |
|------|-----------------|
| User (single-user, local) | Launch apps, search files, use built-in extensions, ask AI questions, customise appearance |

---

## Core Features (Must Have)

### 1. Global Keyboard Shortcut
- Default shortcut: `Alt+Space`
- Works system-wide — even when other apps are focused
- Opens the Flow window centered on screen
- Pressing `Alt+Space` again, or `Escape`, or clicking outside dismisses it instantly
- Window reappears in the same position every time

### 2. App Launcher
- Discovers all installed apps from `C:\ProgramData\Microsoft\Windows\Start Menu\Programs` and `%APPDATA%\Microsoft\Windows\Start Menu\Programs`
- Reads .lnk shortcut files — extracts name, icon, executable path
- Displays app name + icon in results list
- Press `Enter` to launch selected app
- Apps ranked by frequency of use — most opened apps appear first over time

### 3. File Search
- Searches files and folders across the system as the user types
- Uses Windows Search Index via shell command for speed
- Results show: file icon + file name + full path as subtitle
- Press `Enter` to open the file in its default application
- Press `Cmd+Enter` (or `Ctrl+Enter`) to open the containing folder

### 4. Unified Fuzzy Search
- App results and file results appear in the same results list
- Ranked by relevance — apps weighted higher than files for short queries
- Powered by Fuse.js on the frontend
- Results update in under 50ms as user types
- Keyboard navigation: `↑` `↓` arrow keys move through results, `Enter` opens

### 5. Built-in Extensions
Each extension activates when the user types a specific trigger word or pattern:

| Trigger | What It Does |
|---------|-------------|
| `calc 15 * 3` or any math expression | Shows result inline: `= 45` |
| `clip` | Shows clipboard history — last 20 copied items, press Enter to paste |
| `color #FF5733` | Shows color preview swatch + hex, RGB, HSL values |
| `timer 10m` or `timer 30s` | Starts a countdown, shows notification when done |
| `ip` | Shows current local and public IP address |
| `ai [question]` | Sends query to Groq API, streams response in detail panel |

### 6. AI Assistant
- Trigger: type `ai ` followed by any question, or press a dedicated AI shortcut key inside Flow
- Sends query to Groq API (free tier — `llama3-8b-8192` model)
- Response streams word-by-word in the right-side detail panel
- Works for questions, calculations, summaries, code explanations
- Requires internet connection — shows "No internet" gracefully if offline
- API key stored in local config file, never exposed to frontend directly (Tauri handles the call)

### 7. Theme & Appearance Customisation
- Accessed by typing `settings` in the search bar
- Settings panel opens as a second screen within the Flow window

Customisable options:
- **Accent color** — color picker, applies to selected result highlight, active states
- **Theme** — Light / Dark / System (follows Windows setting)
- **Window opacity** — slider 70%–100% (frosted glass effect)
- **Border radius** — Sharp (4px) / Rounded (12px) / Pill (20px)
- **Font size** — Compact (13px) / Comfortable (15px) / Large (17px)
- **Built-in theme presets** — Flow Dark (default), Flow Light, Dracula, Nord, Catppuccin

All settings saved to a local JSON config file. Applied instantly with no restart required.

### 8. Results Detail Panel
- When a result is selected, a right-side panel slides in (if window is wide enough)
- Shows contextual detail:
  - App: name, icon, path, keyboard shortcut to open
  - File: name, path, size, last modified date, preview (text files)
  - AI: streams the response here
  - Extensions: full output (clipboard items, color values, timer controls)
- Panel is optional — user can hide it in settings

---

## AI Feature — Design Detail

The AI feature is built-in, not a plugin. It is a first-class citizen of the launcher.

- Trigger phrase: `ai ` prefix OR dedicated shortcut `Ctrl+Shift+A` inside Flow
- Model: `llama3-8b-8192` via Groq API — chosen for speed and free tier availability
- API call is made from Tauri Rust backend — React frontend never touches the API key
- Response streams token by token into the detail panel
- No conversation history — each query is independent (stateless, simpler, faster)
- Shows a loading indicator (animated dots) while waiting for first token
- Shows "Offline — AI unavailable" if no internet connection detected
- API key set once in settings, stored in local config file

---

## What Flow Is NOT

- ❌ Not a plugin marketplace — no user-installable third-party extensions
- ❌ Not cross-platform at launch — Windows only (Mac support in V2)
- ❌ Not a file manager — cannot move, rename, or delete files
- ❌ Not a browser — does not open URLs or search the web directly (AI can answer web questions)
- ❌ Not a note-taking app
- ❌ Not cloud-synced — all data is local
- ❌ Not multi-user — single user, single machine
- ❌ Not always-visible — it is a popup launcher, not a persistent sidebar
- ❌ No auto-update system at launch

---

## V2 Features (After MVP Ships)

- Mac support (app discovery via `/Applications`, `Spotlight` replacement)
- Web search integration — type `g [query]` to search Google
- Window management commands — `move window left`, `maximize`
- Snippet manager — saved text snippets you can paste anywhere
- Plugin API — let developers build extensions
- Conversation history for AI — multi-turn conversations
- Custom shortcuts — remap any built-in shortcut

---

## Success Criteria

- Flow opens in under 100ms after pressing `Alt+Space`
- Search results appear in under 50ms as the user types
- RAM usage is under 80MB when idle (window hidden)
- RAM usage is under 120MB when open and searching
- Installed apps are discovered correctly on first launch
- AI responds with first token in under 2 seconds on a normal connection
- Theme changes apply instantly with no restart
- The window dismisses immediately on `Escape` or click-outside
