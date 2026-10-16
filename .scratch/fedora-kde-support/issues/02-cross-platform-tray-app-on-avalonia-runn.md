# 02: Cross-platform tray app on Avalonia, running on Fedora KDE and Windows

**What to build:** RokitIgniter runs as a tray-only Avalonia 11.x app on both Fedora 44 KDE Plasma (Wayland session) and Windows. Retarget the .csproj from net8.0-windows to net10.0 and drop UseWindowsForms. Keep OutputType WinExe. Add the Avalonia packages. Copy RokitIgniter.png to the output directory. Set the single-file properties Avalonia's native libraries need, such as IncludeNativeLibrariesForSelfExtract. Bump VersionPrefix and AssemblyVersion to 0.2.0. Remove Properties/Resources.resx and its designer file. Rewrite Program.cs as the composition root, in this order. First, acquire the single-instance guard: a small module that keeps the named mutex "RokitIgniter". Second, pick the tone player for the current OS. Third, build the Igniter with TimeProvider.System and that tone player. Then start the Avalonia app and hand it the Igniter. Reuse the Igniter, tone player port and Windows tone player from Ticket 1; don't add other versions. The Windows tone player is chosen only after an OS check and is marked [SupportedOSPlatform("windows")] so the analyzers stay quiet; add the package that provides SoundPlayer on net10.0 if needed. Add a Linux tone player that implements the same port. Once at startup it picks the first of pw-play, paplay and aplay that it finds on PATH. To play, it starts that player on AppContext.BaseDirectory/10Hz.wav and doesn't wait for it. If no player is found, the app writes a clear stderr message naming the packages to install and exits with a non-zero code before any UI starts. The Avalonia app uses a lifetime that doesn't shut down when the last window closes. It creates a TrayIcon with the tooltip "Rokit Igniter". The icon is the PNG on Linux and the .ico on Windows, both loaded from AppContext.BaseDirectory. A left-click or activate ignites. The NativeMenu items, in order, are Ignite, About and Exit. Ignite does the same as a left-click. On Windows a right-click only opens the menu. About shows "RokitIgniter v{assembly version}". Exit disposes the Igniter, hides the tray icon and shuts down the app lifetime, without calling Environment.Exit. Build one small Avalonia dialog window and use it for both About and the "RokitIgniter is already running." notice. When the guard is already held, the app starts in notice-only mode: it shows the notice and shuts down when the notice closes, and never creates an Igniter or a tray icon.

**Blocked by:** 01

**Status:** done

**Paths:** `RokitIgniter.csproj`, `Program.cs`, `App.axaml`, `App.axaml.cs`, `MessageDialog.axaml`, `MessageDialog.axaml.cs`, `SingleInstanceGuard.cs`, `LinuxTonePlayer.cs`, `WindowsTonePlayer.cs`, `Properties/**`, `README.md`

- [ ] The .csproj targets net10.0 with no OS suffix, has no UseWindowsForms, keeps OutputType WinExe, and has VersionPrefix and AssemblyVersion 0.2.0
- [ ] Properties/Resources.resx and Properties/Resources.Designer.cs are deleted, and nothing references them
- [ ] `dotnet build` and `dotnet test` succeed on Fedora with the .NET 10 SDK; the build gives no platform-compatibility (CA1416) warnings
- [ ] The Program.cs composition root acquires the guard, picks the tone player for the current OS, builds the Igniter with TimeProvider.System, and passes it to the Avalonia app, in that order
- [ ] It reuses the Igniter, tone player port and Windows tone player from Ticket 1; there's no second scheduler or tone player abstraction
- [ ] The Linux tone player picks pw-play, then paplay, then aplay once at startup, and plays AppContext.BaseDirectory/10Hz.wav without waiting on the process
- [ ] With none of the three players on PATH, the app writes a clear message to stderr and exits with a non-zero code
- [ ] Manual check on Fedora 44 KDE, Wayland session: the tray icon shows the PNG artwork and the tooltip "Rokit Igniter"; the tone plays at startup, which you can see in pw-top or on the speaker LED
- [ ] Manual check: a left-click and the Ignite menu item each play the tone right away
- [ ] Manual check: About shows "RokitIgniter v0.2.0.0" in the Avalonia dialog
- [ ] Manual check: Exit removes the tray icon and leaves no process behind, including after an Ignite
- [ ] Manual check: a second launch shows "RokitIgniter is already running." and exits; the first instance keeps running
- [ ] Manual check: the tone plays when the app is started from a different working directory
- [ ] Manual check on Windows: the tray icon is the .ico, a left-click ignites, a right-click only opens the menu, and Ignite, About and Exit work
- [ ] The README describes Linux support (Fedora 44 KDE Plasma, Wayland via XWayland for the dialog), the audio-player requirement (pw-play from pipewire-utils, or paplay or aplay), and building from source with the .NET 10 SDK; it no longer calls the app Windows-only
