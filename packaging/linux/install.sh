#!/bin/bash
# Installs RokitIgniter for the current user (no root needed).
# Usage: ./install.sh [--autostart]
set -euo pipefail

APP_NAME=RokitIgniter
SOURCE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DATA_HOME="${XDG_DATA_HOME:-$HOME/.local/share}"
CONFIG_HOME="${XDG_CONFIG_HOME:-$HOME/.config}"
APP_DIR="$DATA_HOME/$APP_NAME"
APPLICATIONS_DIR="$DATA_HOME/applications"
HICOLOR_DIR="$DATA_HOME/icons/hicolor"
AUTOSTART_DIR="$CONFIG_HOME/autostart"

autostart=false
for arg in "$@"; do
	case "$arg" in
		--autostart) autostart=true ;;
		-h|--help) echo "Usage: $0 [--autostart]"; exit 0 ;;
		*) echo "Unknown option: $arg" >&2; echo "Usage: $0 [--autostart]" >&2; exit 2 ;;
	esac
done

# The icon goes in the hicolor folder for its real pixel size, read from the PNG's IHDR header.
png_size() {
	local width height
	width=$(od -An -tu1 -j16 -N4 "$1" | awk '{ print $1*16777216 + $2*65536 + $3*256 + $4 }')
	height=$(od -An -tu1 -j20 -N4 "$1" | awk '{ print $1*16777216 + $2*65536 + $3*256 + $4 }')
	echo "${width}x${height}"
}

# Lists every directory this script creates, so uninstall.sh can remove them again (and no others).
DIRS_MANIFEST="$APP_DIR/.installed-dirs"
created_dirs=()

make_dir() {
	local dir="$1" missing=()
	while [ ! -d "$dir" ]; do
		missing+=("$dir")
		dir="$(dirname "$dir")"
	done
	mkdir -p "$1"
	created_dirs+=("${missing[@]}")
}

# Copy to a temporary name and rename, so reinstalling over a running copy doesn't fail with "Text file busy".
install_file() {
	local source="$1" target="$2"
	cp "$source" "$target.tmp"
	mv -f "$target.tmp" "$target"
}

# Quote the Exec path per the Desktop Entry spec: wrap in double quotes, escape " ` $ and \.
desktop_exec_quote() {
	local escaped
	escaped=$(printf '%s' "$1" | sed -e 's/[\\"`$]/\\&/g' -e 's/\\/\\\\/g')
	printf '"%s"' "$escaped"
}

make_dir "$APP_DIR"
for file in "$SOURCE_DIR"/*; do
	name="$(basename "$file")"
	case "$name" in
		install.sh|"$APP_NAME.desktop") ;;
		*) install_file "$file" "$APP_DIR/$name" ;;
	esac
done
chmod 755 "$APP_DIR/$APP_NAME" "$APP_DIR/uninstall.sh"

make_dir "$APPLICATIONS_DIR"
exec_line="Exec=$(desktop_exec_quote "$APP_DIR/$APP_NAME")"
exec_line_sed=$(printf '%s' "$exec_line" | sed -e 's/[\\&|]/\\&/g')
sed -e "s|^Exec=.*|$exec_line_sed|" "$SOURCE_DIR/$APP_NAME.desktop" > "$APPLICATIONS_DIR/$APP_NAME.desktop.tmp"
mv -f "$APPLICATIONS_DIR/$APP_NAME.desktop.tmp" "$APPLICATIONS_DIR/$APP_NAME.desktop"

icon_dir="$HICOLOR_DIR/$(png_size "$SOURCE_DIR/$APP_NAME.png")/apps"
make_dir "$icon_dir"
install_file "$SOURCE_DIR/$APP_NAME.png" "$icon_dir/$APP_NAME.png"

if [ "$autostart" = true ]; then
	make_dir "$AUTOSTART_DIR"
	install_file "$APPLICATIONS_DIR/$APP_NAME.desktop" "$AUTOSTART_DIR/$APP_NAME.desktop"
fi

# A reinstall keeps the directories an earlier install recorded.
{
	if [ -f "$DIRS_MANIFEST" ]; then cat "$DIRS_MANIFEST"; fi
	if [ "${#created_dirs[@]}" -gt 0 ]; then printf '%s\n' "${created_dirs[@]}"; fi
} | sort -u > "$DIRS_MANIFEST.tmp"
mv -f "$DIRS_MANIFEST.tmp" "$DIRS_MANIFEST"

# Refresh the desktop and icon caches where the tools exist.
if command -v update-desktop-database > /dev/null; then
	update-desktop-database -q "$APPLICATIONS_DIR" || true
fi
touch "$HICOLOR_DIR"
if command -v gtk-update-icon-cache > /dev/null && [ -e "$HICOLOR_DIR/index.theme" -o -e "$HICOLOR_DIR/icon-theme.cache" ]; then
	gtk-update-icon-cache -q -t -f "$HICOLOR_DIR" || true
fi
if command -v kbuildsycoca6 > /dev/null; then
	kbuildsycoca6 > /dev/null 2>&1 || true
fi

echo "Installed $APP_NAME to $APP_DIR"
if [ "$autostart" = true ]; then
	echo "$APP_NAME will start when you log in."
fi
echo "To uninstall, run $APP_DIR/uninstall.sh"
