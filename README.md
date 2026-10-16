# RokitIgniter
A tray utility for Windows and Linux that keeps KRK Rokit powered speakers from going to sleep.

Many KRK Rokit speakers go to sleep after 30 minutes, and you need to send a fairly strong signal to wake them up again. This is a utility to help keep the speakers active.

This program works by playing a short inaudible 10Hz tone periodically to keep the speakers awake. It just needs to stay running - there is not much to control,
so it simply runs as a tray icon.

The wake-up sound will be played every 25 minutes, but you can invoke it manually by clicking the tray icon, or with Ignite in its menu.

## Linux support

RokitIgniter runs on Fedora 44 KDE Plasma, in the default Wayland session. The tray icon uses KDE's system tray
(StatusNotifierItem over D-Bus). The small About and "already running" dialogs are drawn through XWayland, which
Fedora KDE ships by default. Other desktops with an SNI-capable tray will probably work but aren't tested; GNOME
without an AppIndicator extension has no tray.

The wake tone is played with an external audio player. RokitIgniter uses the first of these it finds on `PATH`:

- `pw-play`, from `pipewire-utils` (installed by default on Fedora 44 KDE)
- `paplay`, from `pulseaudio-utils`
- `aplay`, from `alsa-utils`

If none is installed, RokitIgniter prints a message naming these packages and exits. Install one, for example
`sudo dnf install pipewire-utils`.

## Installing on Linux

Download one of the Linux archives from the [releases page](https://github.com/drittich/RokitIgniter/releases):

- `RokitIgniter-SelfContained-linux-x64-<version>.tar.gz` includes the .NET runtime, so it needs nothing else
  installed. Pick this one if you're not sure.
- `RokitIgniter-linux-x64-<version>.tar.gz` is much smaller but needs the .NET 10 runtime from Fedora's repositories:
  `sudo dnf install dotnet-runtime-10.0`.

Extract it and run the installer. It installs for your user only and doesn't need root:

```sh
tar -xzf RokitIgniter-SelfContained-linux-x64-<version>.tar.gz
cd RokitIgniter-SelfContained-linux-x64-<version>
./install.sh                # or ./install.sh --autostart to also start RokitIgniter when you log in
```

The app goes into `~/.local/share/RokitIgniter`, its launcher into `~/.local/share/applications` and its icon into
`~/.local/share/icons/hicolor`. RokitIgniter then appears in the application launcher. With `--autostart`, the
launcher is also copied into `~/.config/autostart`. To upgrade, run `install.sh` from the newer archive.

To uninstall, choose Exit from the tray menu, then run:

```sh
~/.local/share/RokitIgniter/uninstall.sh
```

This removes everything `install.sh` added, including the autostart entry if there is one.

## Development

Requires the .NET 10 SDK (`dotnet-sdk-10.0` on Fedora, or from https://dot.net on Windows). Build the app and the
tests, then run the tests:

```sh
dotnet build RokitIgniter.sln
dotnet test RokitIgniter.Tests
```

Run the app from source with `dotnet run --project RokitIgniter.csproj`.

The tests in `RokitIgniter.Tests` cover the Igniter's 25-minute cycle with a fake tone player and virtual time,
so they run on Linux and Windows without playing any sound.

To build all four release archives (Windows and Linux) on Fedora, run `./publish.sh <tag>`; see
`Release instructions.txt`.
