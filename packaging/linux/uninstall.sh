#!/bin/bash
# Removes everything install.sh created for the current user.
# Usage: ./uninstall.sh
set -euo pipefail

APP_NAME=RokitIgniter
DATA_HOME="${XDG_DATA_HOME:-$HOME/.local/share}"
CONFIG_HOME="${XDG_CONFIG_HOME:-$HOME/.config}"
APP_DIR="$DATA_HOME/$APP_NAME"
APPLICATIONS_DIR="$DATA_HOME/applications"
HICOLOR_DIR="$DATA_HOME/icons/hicolor"
AUTOSTART_DIR="$CONFIG_HOME/autostart"

DIRS_MANIFEST="$APP_DIR/.installed-dirs"
created_dirs=()
if [ -f "$DIRS_MANIFEST" ]; then
	mapfile -t created_dirs < "$DIRS_MANIFEST"
fi

rm -f "$AUTOSTART_DIR/$APP_NAME.desktop"
rm -f "$APPLICATIONS_DIR/$APP_NAME.desktop"
# install.sh picks the size folder from the PNG, so remove the icon from whichever size folder holds it.
if [ -d "$HICOLOR_DIR" ]; then
	find "$HICOLOR_DIR" -mindepth 3 -maxdepth 3 -path "*/apps/$APP_NAME.png" -type f -delete
fi
rm -rf "$APP_DIR"

# Refresh the desktop and icon caches where the tools exist; the cache files are shared, so they stay.
if [ -d "$APPLICATIONS_DIR" ] && command -v update-desktop-database > /dev/null; then
	update-desktop-database -q "$APPLICATIONS_DIR" || true
fi
if [ -d "$HICOLOR_DIR" ]; then
	touch "$HICOLOR_DIR"
	if command -v gtk-update-icon-cache > /dev/null && [ -e "$HICOLOR_DIR/index.theme" -o -e "$HICOLOR_DIR/icon-theme.cache" ]; then
		gtk-update-icon-cache -q -t -f "$HICOLOR_DIR" || true
	fi
fi
if command -v kbuildsycoca6 > /dev/null; then
	kbuildsycoca6 > /dev/null 2>&1 || true
fi

# Remove the directories install.sh created, deepest first, as long as nothing else has been put in them.
# If install.sh created the applications directory, the mimeinfo.cache it refreshed there is ours too.
if [ "${#created_dirs[@]}" -gt 0 ]; then
	printf '%s\n' "${created_dirs[@]}" | awk '{ print length, $0 }' | sort -rn | cut -d' ' -f2- |
	while IFS= read -r dir; do
		[ -d "$dir" ] || continue
		if [ "$dir" = "$APPLICATIONS_DIR" ] && [ "$(ls -A "$dir")" = "mimeinfo.cache" ]; then
			rm -f "$dir/mimeinfo.cache"
		fi
		rmdir "$dir" 2> /dev/null || true
	done
fi

echo "Uninstalled $APP_NAME"
