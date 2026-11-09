# POE2Radar

[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-blue.svg)](https://www.microsoft.com/windows)
[![Rendering](https://img.shields.io/badge/Render-Direct2D%20%7C%20DirectWrite-success.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

An external, high-performance, and non-intrusive **map, radar, and navigation overlay for Path of Exile 2**.

POE2Radar attaches externally to the game client, reads world and entity states directly from memory without code injection or DLL hooks, and renders hardware-accelerated terrain, navigation paths, and point-of-interest (POI) markers directly over the in-game map.

---

> ⚠️ **Disclaimer & Safety Notice**  
> POE2Radar is an independent, open-source educational project. It operates strictly via read-only memory inspection (`ReadProcessMemory`) and does not modify game binaries, patch instructions, or intercept DirectX render pipelines. However, automating input (such as the opt-in auto-flask feature) or reading game memory may violate the game's Terms of Service. Use responsibly and at your own risk.

---

## Table of Contents

- [Features](#features)
  - [Map Overlay & Live Radar](#map-overlay--live-radar)
  - [Persistent Zone-Wide POI Tracking](#persistent-zone-wide-poi-tracking)
  - [Intelligent A* Navigation & Route Guidance](#intelligent-a-navigation--route-guidance)
  - [Atlas Optimization & Auto-Routing](#atlas-optimization--auto-routing)
  - [Real-Time Economy & Reward Valuation](#real-time-economy--reward-valuation)
  - [Web Dashboard & Local REST API](#web-dashboard--local-rest-api)
  - [Safe Auto-Flask (Opt-in)](#safe-auto-flask-opt-in)
- [Hotkeys & Controls](#hotkeys--controls)
- [Architecture & Tech Stack](#architecture--tech-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Building from Source](#building-from-source)
  - [Publishing a Single-File Release](#publishing-a-single-file-release)
- [Configuration](#configuration)
- [Uploading to GitHub](#uploading-to-github)
- [Credits & Acknowledgments](#credits--acknowledgments)
- [License](#license)

---

## Features

### Map Overlay & Live Radar
- **Walkable Terrain Projection**: Extracts the walkable terrain grid directly from game memory (`TerrainStruct`) and projects it onto the in-game minimap or fullscreen map overlay.
- **Dynamic Entity Tracking**: Displays living monsters, unique bosses, players, NPCs, chests, and interactables in real time.
- **Hardware-Accelerated Direct2D Rendering**: Uses Direct2D and DirectWrite with ClearType subpixel anti-aliasing, rounded glassmorphic HUD elements, and drop-shadowed typography for maximum legibility.
- **Monster Health & Shield Bars**: Optional world-space HP/Energy Shield bars anchored above enemies, color-coded by rarity (Normal, Magic, Rare, Unique).

### Persistent Zone-Wide POI Tracking
- **Dual Entity Map Traversal**: PoE2 moves distant entities out of the active simulation bubble (~150 grid distance) into `SleepingEntities` (`0x728`). POE2Radar traverses both `AwakeEntities` (`0x718`) and `SleepingEntities` (`0x728`), ensuring that points of interest across the entire map are detected immediately.
- **Persistent Zone POI Memory**: Discovered waypoints, town portals, NPC locations, shrines, chests, monoliths, and area transitions are permanently cached for the entire lifetime of the zone. They remain visible on your map overlay regardless of how far your character travels.
- **Automatic Instance Cleanup**: Flushes and re-initializes memory caches seamlessly whenever you change areas or enter a new map instance.

### Intelligent A* Navigation & Route Guidance
- **Off-Thread Asynchronous Replanner**: Runs A* pathfinding on a dedicated background worker (`BackgroundReplanner`), guaranteeing zero frame drops or input stutter on the render thread.
- **Dual-Layer Guidance Routes**: Renders high-visibility glowing polyline paths with dark drop-shadow outlines on both the minimap and the in-game world ground.
- **In-World Destination Beacons**: Displays vertical destination light pillars and ground markers at selected goals.
- **Multi-Target Routing**: Select multiple destinations simultaneously. Each route is automatically assigned a distinct palette color with hop distance tracking.
- **Zone Selection Memory**: Remembers your active target selections when making round-trips to town, restoring your paths upon return.

### Atlas Optimization & Auto-Routing
- **Interactive Atlas Overlay**: Highlights and categorizes Atlas map nodes by content type (Breach, Ritual, Delirium, Expedition, Bosses, Citadels, Towers, Unique maps).
- **Off-Screen Target Chevrons**: Directional arrows point toward tracked high-value maps outside your current viewport.
- **Shortest-Hop Pathing**: Computes shortest-hop connection routes from accessible map frontiers to target objectives.

### Real-Time Economy & Reward Valuation
- **Live poe.ninja Integration**: Periodically updates item prices from poe.ninja to display real-time Divine/Chaos values for dropped items.
- **Unidentified Unique Reveal**: Instantly identifies dropped unidentified unique items from memory metadata and displays their true identity and market value.
- **Pre-Reveal Encounter Pricing**:
  - **Ritual Altar**: Surfaces item values on tribute-shop tiles before rerolling.
  - **Expedition Monoliths**: Previews the best reward, hole count, and anchor rune before opening the station.
  - **Runeforge Combinations**: Evaluates crafting recipe outputs and rune combinations.

### Web Dashboard & Local REST API
- Runs an embedded HTTP server locally on `http://localhost:7777` (open with **F12**).
- **Live Navigator Tab**: Interactive browser interface to view all discovered entities and landmarks with one-click navigation targeting.
- **Custom Display Rules Engine**: Create, reorder, and tweak custom icon shapes, colors, sizes, and labels based on category, metadata substring/glob, or monster affixes.
- **REST Endpoints**:
  - `GET /state` — Current player position, vitals, active area, entity counts, and zoom.
  - `GET /entities` — Live list of detected entities and coordinates.
  - `GET /landmarks` — Static tile landmarks and cluster centroids.
  - `GET /api/nav` — Active navigation targets and route states.
  - `POST /api/nav/toggle` — Remotely select or deselect navigation targets.

### Safe Auto-Flask (Opt-in)
- **Multi-Resource Triggers**: Monitors Health, Energy Shield, and Mana percentage thresholds independently.
- **Strictly Foreground-Gated**: Uses Win32 `GetForegroundWindow` to ensure keystrokes are ONLY sent when `PathOfExileSteam.exe` is the active, focused window.
- **Death & Loading Guard**: Automatically halts flask inputs when character health is 0, during loading screens, or during zone transitions.
- **Instant Kill-Switch**: Toggle on/off instantly at any time by pressing **F8**.

---

## Hotkeys & Controls

| Hotkey | Action | Description |
| :--- | :--- | :--- |
| **`F6`** | **Add Nearest Target** | Automatically selects and routes to the closest unselected landmark or POI |
| **`F7`** | **Clear Routes** | Clears all active navigation destinations and route lines |
| **`F8`** | **Toggle Auto-Flask** | Instant kill-switch / enable for the automated flask system |
| **`F9`** | **Exit Radar** | Cleanly shuts down the overlay and releases all memory hooks/handles |
| **`F10`** | **Atlas Inspector** | (With Atlas open) Inspects hovered map node and sets route start/finish |
| **`F12`** | **Open Dashboard** | Opens the local web control panel (`http://localhost:7777`) in your default browser |

---

## Architecture & Tech Stack

```
POE2Radar/
├── src/
│   ├── POE2Radar.Core/          # Low-level memory reading, offsets, and algorithms
│   │   ├── Game/                # Offset tables (Poe2Offsets.cs), entity reading, live state
│   │   ├── Native/              # Win32 memory APIs (OpenProcess, ReadProcessMemory)
│   │   └── Pathfinding/         # Grid extraction, A* search, and polyline smoothing
│   ├── POE2Radar.Overlay/       # Overlay runtime, UI rendering, and API
│   │   ├── Config/              # Settings persistence and JSON serialization
│   │   ├── Overlay/             # Direct2D/DirectWrite rendering and click-through handling
│   │   ├── Pricing/             # poe.ninja price caching and ground-item valuation
│   │   └── Web/                 # Local HTTP server, REST endpoints, and dashboard
│   └── POE2Radar.Research/      # Diagnostic probes, memory scanners, and AOB pattern validators
├── config/                      # User configuration and custom display rules
├── publish.ps1                  # Self-contained release publisher script
└── POE2Radar.slnx               # Modern solution file (.NET 10)
```

- **Runtime**: .NET 10.0 (C# 13)
- **Graphics API**: Direct2D 1.1, DirectWrite, DXGI (`Vortice.Direct2D1` / `Vortice.Mathematics`)
- **OS**: Windows 10 / 11 x64
- **Process Memory Engine**: Read-only Win32 API (`KERNEL32.dll!ReadProcessMemory`)

---

## Getting Started

### Prerequisites
- **Operating System**: Windows 10 / 11 (64-bit)
- **Game Mode**: Path of Exile 2 set to **Windowed** or **Borderless Windowed** (exclusive fullscreen prevents transparent overlays from rendering on top)
- **Development**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/<your-username>/POE2Radar.git
   cd POE2Radar
   ```

2. Build the solution using the .NET CLI:
   ```bash
   dotnet build POE2Radar.slnx -c Release
   ```

3. Launch with Path of Exile 2 running:
   ```bash
   # Run as Administrator
   .\src\POE2Radar.Overlay\bin\Release\net10.0-windows\POE2Radar.Overlay.exe
   ```

### Publishing a Single-File Release

To produce a self-contained, single-file executable with zero runtime dependencies:

```powershell
powershell -ExecutionPolicy Bypass -File .\publish.ps1 v1.0.0
```

The compiled release package will be generated at `publish/POE2Radar.Overlay.exe` and archived to `POE2Radar-v1.0.0-win-x64.zip`.

---

## Configuration

Settings are saved automatically as human-readable JSON files in the `config/` directory:

- `radar_settings.json`: General overlay options, frame rate caps, auto-flask parameters, and zoom settings.
- `display_rules.json`: Ordered entity filtering rules, icon definitions, colors, and visibility gates.
- `hidden_entities.json`: User-defined entity patterns to ignore.
- `price_cache.json`: Cached market rates and currency values from poe.ninja.

---

## Uploading to GitHub

To push this codebase to your own GitHub account:

### Step 1: Create a New GitHub Repository
1. Go to [GitHub - New Repository](https://github.com/new).
2. Enter a repository name (e.g. `POE2Radar`).
3. Set the repository to **Public** or **Private**.
4. **Important**: Do **not** check "Initialize this repository with a README, .gitignore, or license" (we already have all of these prepared).
5. Click **Create repository**.

### Step 2: Initialize & Push from Your Machine

Open PowerShell, navigate to the project directory, and execute:

```powershell
# 1. Navigate to the project root directory
cd "C:\Users\Administrator\Documents\TST\POE@"

# 2. Initialize git (if not already initialized)
git init -b main

# 3. Stage all source files (excluding build artifacts ignored by .gitignore)
git add .

# 4. Commit your files
git commit -m "Initial commit: POE2Radar overlay with persistent POIs and modern UI"

# 5. Link your GitHub repository (replace with your actual GitHub repo URL)
git remote add origin https://github.com/<YOUR-USERNAME>/<YOUR-REPOSITORY>.git

# 6. Push to GitHub
git branch -M main
git push -u origin main
```

*(If you already have a remote configured, you can update it with `git remote set-url origin https://github.com/<YOUR-USERNAME>/<YOUR-REPOSITORY>.git` before pushing.)*

---

## Credits & Acknowledgments

- **GameHelper2 Project**: Core memory structure layout insights, component conventions, and research methodologies.
- **poe.ninja**: Market rate feeds and item valuation data.
- **Vortice.Windows**: Direct2D and DirectX bindings for .NET.

---

## License

This project is licensed under the [MIT License](LICENSE).
