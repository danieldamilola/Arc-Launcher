# PLAN.md — Flow Build Roadmap

## Phase 1: Foundation ✅
- [x] Scaffold Vite + React + TypeScript project
- [x] Install all dependencies (Zustand, Fuse.js, Lucide React, react-window)
- [x] Scaffold Tauri 2.x Rust backend with all command stubs
- [x] Set up design tokens (colors, spacing, fonts, radius, shadows)
- [x] Set up CSS variables and global styles
- [x] Create Zustand store with all actions
- [x] Create typed Tauri command wrappers
- [x] Create all component stubs
- [x] Create all hook stubs
- [x] Create all extension stubs
- [x] Set up CSS Modules for all components

## Phase 2: Search & Launch
- [ ] Implement app discovery (Start Menu .lnk parsing)
- [ ] Implement file search (Windows Search Index)
- [ ] Wire up Fuse.js fuzzy search
- [ ] Implement frequency-based ranking
- [ ] Implement keyboard navigation (arrow keys, Enter, Escape)
- [ ] Implement app/file launching
- [ ] Implement global shortcut registration (Alt+Space)

## Phase 3: Extensions
- [ ] Implement Calculator extension
- [ ] Implement Clipboard history extension
- [ ] Implement Color Picker extension
- [ ] Implement Timer extension with notifications
- [ ] Implement IP Address extension
- [ ] Implement Settings panel (theming)

## Phase 4: AI Assistant
- [ ] Implement Groq API call from Rust backend
- [ ] Implement streaming token response
- [ ] Implement AI chat UI in detail panel
- [ ] Handle offline / API key missing states

## Phase 5: Polish
- [ ] Theme presets (Flow Dark, Flow Light, Dracula, Nord, Catppuccin)
- [ ] Window animations (appear, dismiss)
- [ ] Detail panel slide-in
- [ ] Font loading (Inter + JetBrains Mono)
- [ ] App icons
- [ ] Performance profiling (sub-50ms search, sub-100ms open)
- [ ] RAM profiling (<80MB idle, <120MB active)
