# Architecture

RokitIgniter is a tray-only Avalonia app on `net10.0`: no main window, one scheduler, a few thin platform adapters. It
runs on Windows and on Linux (Fedora 44 KDE Plasma). Behaviour is specified in `.scratch/fedora-kde-support/spec.md`;
vocabulary is in `GLOSSARY.md`; the design terms (module, interface, seam, adapter, depth) follow the
`codebase-design` skill.

## Modules

```
Program.cs (composition root)
  ├─ SingleInstanceGuard ──── named mutex "RokitIgniter" (per session on Windows, per user on Linux)
  ├─ ITonePlayer  ◄──────────┬─ WindowsTonePlayer  (SoundPlayer)          [windows only]
  │                          └─ LinuxTonePlayer    (pw-play|paplay|aplay) [linux]
  ├─ Igniter(ITonePlayer, TimeProvider, interval = 25 min)
  └─ App (Avalonia): TrayIcon + NativeMenu, MessageDialog
```

| Module | File(s) | Interface |
| --- | --- | --- |
| **Igniter** | `Igniter.cs` | `new Igniter(ITonePlayer, TimeProvider, TimeSpan? interval)` plays the wake tone now and starts the cycle; `IgniteAsync()` plays now and restarts the cycle; `Dispose()` stops the cycle and waits for it. Tone player exceptions are logged and never end the cycle. Must not deadlock when `Dispose` is called synchronously on the UI thread (no resuming on the caller's sync context). |
| **Tone player port** | `ITonePlayer.cs` | One operation: play the wake tone once. No scheduling, no waiting on playback required. |
| **Windows tone player** | `WindowsTonePlayer.cs` | Wraps `SoundPlayer` (from the `System.Windows.Extensions` package); WAV at `AppContext.BaseDirectory/10Hz.wav`. `[SupportedOSPlatform("windows")]`. |
| **Linux tone player** | `LinuxTonePlayer.cs` | `LinuxTonePlayer.FindOnPath()` picks the first of `pw-play`, `paplay`, `aplay` on `PATH` once at startup, or returns null; `Play()` starts it on the WAV without waiting. On null the composition root writes to stderr and exits non-zero before any UI. |
| **Single-instance guard** | `SingleInstanceGuard.cs` | `new SingleInstanceGuard(currentSessionOnly)`: acquire once at startup; `IsFirstInstance` tells the caller whether this process is the first instance; held for the process lifetime. The mutex is always private to the current user; its session scope is chosen per OS in `Program.cs`: **per session on Windows** (`currentSessionOnly: true`, the `Local\` namespace, so fast user switching and RDP sessions don't block each other), **shared across sessions on Linux** (`false`, because each shell is its own session there, so a launcher start and a terminal start must see each other). |
| **Avalonia app** | `App.axaml(.cs)`, `MessageDialog.axaml(.cs)` | Receives an `Igniter` (null means notice-only mode). Tray tooltip "Rokit Igniter"; left-click/activate ignites; menu Ignite, About, Exit. Exit disposes the Igniter, hides the tray icon and shuts the lifetime down (no `Environment.Exit`). One dialog for About and the "already running" notice. |
| **Composition root** | `Program.cs` | In order: guard → pick tone player for OS → `new Igniter(player, TimeProvider.System)` → start Avalonia with it. The only place with OS checks. |
| **Packaging** | `publish.sh`, `publish.bat`, `packaging/linux/**` (`RokitIgniter.desktop`, `install.sh`, `uninstall.sh`) | `./publish.sh vX.Y.Z` → 4 archives in `bin/Publish` (Windows and Linux, framework-dependent and self-contained); `publish.bat` builds the two Windows archives on Windows. Per-user Linux install, no root. |

## Seams

- **The Igniter is the one test seam.** Its interface (construct, `IgniteAsync`, `Dispose`) is the test surface. Tests
  substitute both dependencies: a fake tone player that records plays, and `FakeTimeProvider` for virtual time.
  Assert on *tone plays over time*, never on private fields, tokens or timers.
- **The tone player port is a real seam**: two adapters (Windows, Linux) vary across it, plus the test fake.
- Everything else (tray, dialogs, guard, adapters, install scripts) is thin platform glue, covered by the manual smoke
  checklist in the spec, not by automated tests.

## Where new code belongs

- Shared, platform-neutral logic: next to `Igniter.cs` at the repo root (the app project is flat; keep it flat).
  It must not touch Avalonia, `System.Media`, processes or the file system directly, so the test project can link it.
- Platform-specific code: its own `*TonePlayer.cs` / guard file, selected only in `Program.cs`. Mark Windows-only
  types `[SupportedOSPlatform("windows")]`.
- UI: `App.axaml*` and `MessageDialog.axaml*` only.
- Bundled assets (`10Hz.wav`, `RokitIgniter.png`, `RokitIgniter.ico`): copied to output and located through
  `AppContext.BaseDirectory`, never `Environment.CurrentDirectory`.
- Linux packaging files: `packaging/linux/`. Release scripts: repo root (`publish.sh`, `publish.bat`).
- Tests: `RokitIgniter.Tests/`. See the test-harness rules in `CLAUDE.md`.
