# 03: Linux release archives with per-user install and uninstall

**What to build:** The maintainer can cut a full release from Fedora, and a Fedora KDE user can install it without root. Add a bash publish script, publish.sh. Like publish.bat, it takes the version tag as an argument and produces four archives in bin/Publish. The two Windows archives are win-x64 single-file builds, one framework-dependent and one self-contained, in the same format and with the same names as publish.bat produces today. The two Linux archives are linux-x64 single-file builds, one framework-dependent and one self-contained, packed as .tar.gz. Each Linux archive contains the executable, 10Hz.wav, RokitIgniter.png, a RokitIgniter.desktop file, install.sh and uninstall.sh. The .desktop entry has Type=Application, Name=RokitIgniter, Categories=AudioVideo;Audio;Utility; and the RokitIgniter icon. install.sh writes its Exec line with the absolute path of the installed executable. install.sh installs for the current user only. It copies the app files into ~/.local/share/RokitIgniter, installs the .desktop file into ~/.local/share/applications and the PNG into the hicolor icon theme under ~/.local/share/icons, in the folder for the PNG's real pixel size. It refreshes the desktop and icon caches if the tools are available. With --autostart, it also copies the .desktop file into ~/.config/autostart. uninstall.sh removes everything install.sh created, including the autostart entry when there is one. Leave publish.bat as it is. This Ticket doesn't change app code; it packages the build from Ticket 2.

**Blocked by:** 02

**Status:** done

**Paths:** `publish.sh`, `packaging/linux/**`, `Release instructions.txt`, `README.md`

- [ ] `./publish.sh v0.2.0` on Fedora produces exactly four archives: two Windows (framework-dependent and self-contained single-file, win-x64) and two Linux .tar.gz (framework-dependent and self-contained single-file, linux-x64)
- [ ] The Windows archives have the same format, names and contents as publish.bat produces
- [ ] Each Linux archive contains the executable, 10Hz.wav, RokitIgniter.png, RokitIgniter.desktop, install.sh and uninstall.sh
- [ ] The self-contained Linux build starts on a Fedora 44 KDE machine without the .NET runtime installed; the framework-dependent build starts with Fedora's dotnet-runtime-10.0
- [ ] install.sh needs no root and installs into ~/.local/share/RokitIgniter, ~/.local/share/applications and the user's hicolor icon directory
- [ ] The installed .desktop file has Type=Application, Categories=AudioVideo;Audio;Utility;, and an Exec line with the absolute path of the installed executable
- [ ] `install.sh --autostart` also puts the .desktop file into ~/.config/autostart
- [ ] uninstall.sh removes every file and directory install.sh created and nothing else
- [ ] Manual check on Fedora 44 KDE: after install, RokitIgniter appears in the application launcher with its icon and starts from there; with --autostart it starts after logging in again; after uninstall all of it is gone
- [ ] publish.bat still works unchanged for Windows-only builds
- [ ] 'Release instructions.txt' says to run publish.sh with the tag, attach all four archives, and do the manual smoke checklist on Fedora 44 KDE and Windows before publishing
- [ ] The README covers downloading the Linux archive, choosing between the self-contained and framework-dependent builds, running install.sh (with --autostart as an option) and uninstall.sh
