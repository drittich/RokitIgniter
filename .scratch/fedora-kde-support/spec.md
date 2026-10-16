# Spec: Run RokitIgniter on Fedora 44 KDE Plasma

Status: ready-for-agent

## Problem Statement

RokitIgniter keeps KRK Rokit powered speakers from going to sleep. Every 25 minutes it plays a short, inaudible 10Hz **wake tone** from a tray icon. It only runs on Windows today: it uses Windows Forms for the tray icon and dialogs, `SoundPlayer` (Windows-only) for audio, and a Windows-only publish script that produces Windows binaries.

The user's main desktop is Fedora 44 with KDE Plasma (Plasma 6, Wayland session, PipeWire audio). With nothing like RokitIgniter on that machine, the speakers fall asleep after about 30 minutes of silence, and the user has to wake them by hand. They need the same small tray utility, with the same behaviour, built and released for Fedora KDE as well as Windows.

## Solution

Make RokitIgniter a single cross-platform tray application that builds and runs on both Windows and Linux (Fedora 44 KDE Plasma is the target Linux desktop).

On Fedora KDE the user downloads a Linux release archive, runs an included install step, and gets:

- a RokitIgniter tray icon in the Plasma system tray
- the wake tone on startup and every 25 minutes after that
- a left-click on the tray icon that **ignites**, meaning it plays the wake tone right away and restarts the 25-minute cycle
- a right-click menu with Ignite, About, and Exit
- a guard that refuses a second running instance
- a launcher entry in the application menu, and an optional entry that starts it automatically at login

Windows behaviour stays the same as today.

## User Stories

1. As a Fedora KDE user, I want RokitIgniter to run on my Linux desktop, so that my KRK Rokit speakers stay awake while I work.
2. As a Fedora KDE user, I want a RokitIgniter icon in the Plasma system tray, so that I know the app is running.
3. As a Fedora KDE user, I want the tray icon to show the RokitIgniter artwork, so that I can tell it apart from other tray icons.
4. As a Fedora KDE user, I want a tooltip reading "Rokit Igniter" when I hover the tray icon, so that I can identify it.
5. As a Fedora KDE user, I want the wake tone to play as soon as the app starts, so that speakers that are already asleep wake up straight away.
6. As a Fedora KDE user, I want the wake tone to play every 25 minutes, so that the speakers never reach their 30-minute sleep timeout.
7. As a Fedora KDE user, I want left-clicking the tray icon to ignite, so that I can wake the speakers on demand.
8. As a Fedora KDE user, I want a manual ignite to restart the 25-minute countdown, so that the next automatic tone comes 25 minutes after my manual one.
9. As a Fedora KDE user, I want a right-click context menu with Ignite, About, and Exit, so that I have the same controls as on Windows.
10. As a Fedora KDE user, I want the Ignite menu item to behave exactly like a left-click, so that both ways of igniting are consistent.
11. As a Fedora KDE user, I want About to show the app name and version, so that I know which release I'm running.
12. As a Fedora KDE user, I want Exit to stop the timer, remove the tray icon, and end the process cleanly, so that nothing is left running in the background.
13. As a Fedora KDE user, I want a second launch to tell me RokitIgniter is already running and then quit, so that two copies never play overlapping tones.
14. As a Fedora KDE user, I want the wake tone to play through my default PipeWire output, so that it reaches the speakers I've picked in Plasma's audio settings.
15. As a Fedora KDE user, I want the wake tone to play even when the app was started from the application menu or at login, whatever the working directory is, so that it works however it was launched.
16. As a Fedora KDE user, I want the app to work in the default Plasma Wayland session, so that I don't have to switch to X11.
17. As a Fedora KDE user, I want a RokitIgniter entry in the Plasma application launcher, so that I can start it like any other app.
18. As a Fedora KDE user, I want an optional autostart entry, so that RokitIgniter starts automatically when I log in.
19. As a Fedora KDE user, I want a simple install step that puts the app, its icon, and its desktop entries in my user profile, so that I don't need root or a package manager.
20. As a Fedora KDE user, I want a matching uninstall step, so that I can remove everything the installer added.
21. As a Fedora KDE user, I want a self-contained Linux build, so that I can run the app without installing the .NET runtime.
22. As a Fedora KDE user who already has the .NET runtime from Fedora's repositories, I want a smaller framework-dependent build, so that I can download less.
23. As a Fedora KDE user, I want a clear message on stderr and a non-zero exit code if no supported audio player is found, so that I know what to install.
24. As a Fedora KDE user, I want a failed tone playback to be logged without crashing the app, so that one audio hiccup doesn't stop future wake tones.
25. As a Fedora KDE user, I want the app to use very little CPU and memory while it waits, so that it doesn't slow down my desktop.
26. As a Windows user, I want RokitIgniter to keep behaving as it does now, so that the Linux work doesn't break my setup.
27. As a Windows user, I want the Windows release archives to keep their current form (framework-dependent and self-contained single-file builds), so that my update process stays the same.
28. As the maintainer, I want one codebase for both platforms, so that a behaviour change only has to be made once.
29. As the maintainer, I want platform-specific code (audio playback, single-instance guard, packaging) behind small, clearly named adapters, so that the shared logic stays platform-neutral.
30. As the maintainer, I want automated tests for the scheduling logic, so that I can change it with confidence on either OS.
31. As the maintainer, I want to build and run the tests on my Fedora machine with the .NET SDK, so that I don't need Windows to develop.
32. As the maintainer, I want a Linux-friendly publish script that produces every release archive (Windows and Linux), so that I can cut a release from Fedora.
33. As the maintainer, I want the release instructions updated to cover the Linux archives, so that every release includes them.
34. As the maintainer, I want the README to describe Linux support, installation, and autostart, so that Linux users can get started without asking me.
35. As the maintainer, I want the app on a currently supported .NET LTS version, so that it keeps getting security fixes (.NET 8 support ends in November 2026).

## Implementation Decisions

**Platform and framework**

- Move from Windows Forms to **Avalonia UI** (11.x) as the single cross-platform UI layer. Avalonia's `TrayIcon` with a `NativeMenu` uses the StatusNotifierItem D-Bus protocol on Linux, which KDE Plasma supports natively, and the native shell tray on Windows. This replaces `NotifyIcon` and `ContextMenuStrip`.
- The app stays tray-only and has no main window. It runs an Avalonia application lifetime that does not shut down when the last window closes, and shuts down explicitly on Exit.
- Retarget from `net8.0-windows` to the current .NET LTS (`net10.0`), with no OS suffix. Drop `UseWindowsForms`. The output type stays `WinExe`, so no console window appears on Windows.
- Windows Forms `MessageBox` calls (the About dialog and the "already running" notice) become one small Avalonia dialog window that is reused for both. Avalonia has no built-in message box, so this dialog is part of the app.
- Avalonia renders through XWayland under Plasma's Wayland session. That only affects the small dialog. The tray icon goes over D-Bus and doesn't depend on the display server. Fedora KDE ships XWayland by default.
- Remove the unused template resources (the `Resources.resx` and designer pair with placeholder string, colour, bitmap, and icon entries) as part of the port.

**Igniter (the shared scheduler, which is also the test seam)**

- The Igniter keeps its current behaviour: it plays the wake tone right away when started, then every 25 minutes. Ignite cancels the current cycle, plays the tone right away, and starts a fresh 25-minute cycle. Dispose stops the cycle and waits for it to finish.
- The Igniter receives its dependencies instead of creating them:
  - a **tone player** port, a single-operation interface that plays the wake tone once. It knows nothing about scheduling.
  - a **TimeProvider**, used to build its periodic timer. In production this is `TimeProvider.System`.
- The interval stays at a fixed 25 minutes. Making it configurable is out of scope, but the constructor may accept the interval with 25 minutes as the default, so tests can state it explicitly.
- If the tone player throws, the Igniter logs the error (stderr or debug output is enough) and keeps its schedule. A failure must not end the timer loop.

**Tone player adapters**

- **Windows adapter**: wraps the existing `SoundPlayer` behaviour. It's compiled or selected only on Windows (an OS check at the composition root, with platform attributes so the analyzers stay quiet).
- **Linux adapter**: plays the bundled WAV by starting an external player process without waiting on it. Players are tried in order: `pw-play` (PipeWire, installed by default on Fedora 44 KDE through `pipewire-utils`), then `paplay` (PulseAudio compatibility), then `aplay` (ALSA). The first one found on `PATH` is chosen once at startup. If none is found, the app writes a clear error to stderr and exits with a non-zero code.
- Both adapters locate the WAV relative to the application's base directory (`AppContext.BaseDirectory`), not the current working directory. This fixes a latent bug where launching from a shortcut, autostart, or the app launcher with a different working directory fails to find `10Hz.wav`. The same fix applies to the tray icon image.

**Single-instance guard**

- Keep the named-mutex approach. .NET supports named mutexes on Linux across processes owned by the same user, so the current mechanism carries over. The guard is a small module called once at startup. When another instance holds it, the app shows the "already running" dialog and exits.

**Tray icon and menu**

- Tooltip: "Rokit Igniter".
- Icon: the existing PNG artwork on Linux. The existing `.ico` stays as the Windows executable and tray icon.
- Left-click / activate → Ignite. Menu items in order: Ignite, About, Exit. Right-click on Windows still only opens the menu and does not ignite, which matches the current code.
- Exit disposes the Igniter, hides the tray icon, and shuts down the application lifetime. It does not call `Environment.Exit`.
- About shows "RokitIgniter v{assembly version}".

**Composition root**

- Program startup does three things: acquire the single-instance guard; pick the tone player adapter for the current OS; build the Igniter with `TimeProvider.System` and that adapter. It then starts the Avalonia app and hands it the Igniter.

**Packaging and release**

- Add a bash publish script, usable on Fedora, that produces four archives. The version tag is passed as an argument, like `publish.bat` takes it today:
  - Windows framework-dependent single-file (`win-x64`), the same as the current regular build
  - Windows self-contained single-file (`win-x64`), the same as the current self-contained build
  - Linux framework-dependent single-file (`linux-x64`), as `.tar.gz`
  - Linux self-contained single-file (`linux-x64`), as `.tar.gz`
- Keep `publish.bat` working for Windows builds, or replace it with a note pointing to the new script. Either is fine, as long as the release instructions match.
- Each Linux archive contains the executable, `10Hz.wav`, the PNG icon, a `.desktop` file, and `install.sh` / `uninstall.sh` scripts.
- `install.sh` installs for the current user only and needs no root:
  - app files into a per-user directory under `~/.local/share` (or `~/.local/opt`)
  - the `.desktop` launcher into `~/.local/share/applications`
  - the icon into the user's hicolor icon theme directory
  - with an `--autostart` flag, a copy of the `.desktop` file into `~/.config/autostart`
- `uninstall.sh` removes everything `install.sh` created.
- The `.desktop` entry is `Type=Application`, has `Categories=AudioVideo;Audio;Utility;`, and uses an `Exec` line that points at the installed executable.
- Bump the version prefix and assembly version to 0.2.0, since the framework change is user-visible.
- Update the README (Linux support, install, autostart, and the audio-player requirement) and the release instructions (run the new script, attach all four archives).

## Testing Decisions

- **One test seam: the Igniter's public interface** (start on construction, Ignite, Dispose), with a fake tone player and a fake time provider (`FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`). Tests drive virtual time forward and assert how many times, and when, the fake tone player was called. They must not inspect the Igniter's private state, cancellation tokens, or timer objects.
- A good test here describes behaviour a user would notice, in terms of tone plays over time:
  - Exactly one tone plays straight after construction.
  - No further tone plays before 25 minutes of virtual time pass, and exactly one plays when they do.
  - Over N × 25 minutes, N + 1 tones play.
  - Ignite plays a tone right away.
  - After Ignite at minute 10, the next automatic tone comes at minute 35, not minute 25.
  - Several quick Ignite calls each play one tone and leave a single active cycle, never several overlapping ones.
  - After Dispose, advancing time plays no further tones, and Dispose returns without hanging.
  - If the tone player throws, later scheduled tones still play.
- Add a new xUnit test project next to the app in the solution. It must build and run with `dotnet test` on both Fedora and Windows, so it must not reference Avalonia or any OS-specific API.
- There are no existing tests in the repo, so there is no prior art. These tests set the pattern.
- **Not covered by automated tests:** the tray icon, menu, dialogs, the real tone player adapters, the single-instance guard, and the install scripts. Each of these is thin platform glue. Cover them with a manual smoke checklist on Fedora 44 KDE (Wayland session) and on Windows before each release:
  - The tray icon appears with its tooltip.
  - A left-click plays the tone (check the speaker LED or `pw-top`).
  - The menu items work.
  - A second launch shows the notice and exits.
  - Exit leaves no process behind.
  - Install, then a launcher start, an autostart after re-login, and an uninstall all work.

## Out of Scope

- macOS support.
- Linux architectures other than x86-64 (for example `linux-arm64`).
- Distribution packages and stores: RPM, COPR, Flatpak/Flathub, AppImage, Snap.
- Guaranteed support for desktops other than KDE Plasma. GNOME without an AppIndicator/SNI extension has no system tray. Other SNI-capable desktops will probably work, but nobody will test them.
- A configurable interval, a configurable tone, or a settings UI.
- A Windows installer or autostart setup on Windows (unchanged from today).
- CI workflows for building, testing, or releasing.
- Native Wayland rendering for the dialog (XWayland is acceptable).

## Further Notes

- Glossary used in this spec:
  - **wake tone**: the bundled 10Hz WAV.
  - **ignite**: play the wake tone right away and restart the 25-minute cycle.
  - **tone player**: the platform adapter that plays the wake tone once.
- The 10Hz tone passes through PipeWire's default sink at its native sample rate. When smoke testing, confirm it still wakes the speakers. If PipeWire's processing were to weaken it in a way that matters, the fallback is to try `paplay` or to set the target sink explicitly. Neither is expected to be needed.
- .NET 8 reaches end of support in November 2026, which is one reason to retarget to .NET 10 LTS. Fedora 44 provides the .NET 10 SDK and runtime from its own repositories (`dotnet-sdk-10.0`), which is enough to build on Fedora and to run the framework-dependent Linux build.
- The current Exit handler calls `Environment.Exit(0)` without disposing the Igniter. The port fixes this in passing (see Tray icon and menu).
