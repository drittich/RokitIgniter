#!/bin/bash
# Builds all four release archives into bin/Publish.
# Usage: ./publish.sh [tag]    e.g. ./publish.sh v0.2.0 (no dash)
set -euo pipefail

EXE_VERSION="${1:+-$1}"
APP_NAME=RokitIgniter
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PUBLISH_PATH="$PROJECT_PATH/bin/Publish"
TEMP_PATH="$PUBLISH_PATH/TEMP"

command -v 7z > /dev/null || { echo "7z not found; install it with: sudo dnf install 7zip" >&2; exit 1; }

rm -rf "$PUBLISH_PATH"
mkdir -p "$PUBLISH_PATH"

# dotnet publish of the app project (not the solution, which also holds the tests) into TEMP
publish() {
	local runtime="$1" self_contained="$2"
	rm -rf "$TEMP_PATH"
	dotnet publish "$PROJECT_PATH/$APP_NAME.csproj" --nologo -r "$runtime" -c Release --self-contained "$self_contained" -p:PublishSingleFile=true -o "$TEMP_PATH"
}

# Windows archives: the same names and layout as publish.bat (files at the top level of a .7z)
windows() {
	local self_contained="$1" archive="$2"
	publish win-x64 "$self_contained"
	(cd "$TEMP_PATH" && 7z a -t7z -r -mx9 "$PUBLISH_PATH/$archive.7z" ./* > /dev/null)
	rm -rf "$TEMP_PATH"
}

# Linux archives: the publish output plus the desktop entry and install scripts, in one top-level folder
linux() {
	local self_contained="$1" archive="$2"
	publish linux-x64 "$self_contained"
	cp "$PROJECT_PATH/packaging/linux/$APP_NAME.desktop" "$PROJECT_PATH/packaging/linux/install.sh" "$PROJECT_PATH/packaging/linux/uninstall.sh" "$TEMP_PATH/"
	chmod 755 "$TEMP_PATH/$APP_NAME" "$TEMP_PATH/install.sh" "$TEMP_PATH/uninstall.sh"
	mv "$TEMP_PATH" "$PUBLISH_PATH/$archive"
	tar -czf "$PUBLISH_PATH/$archive.tar.gz" -C "$PUBLISH_PATH" "$archive"
	rm -rf "$PUBLISH_PATH/$archive"
}

echo "publishing regular version..."
windows false "$APP_NAME$EXE_VERSION"

echo "publishing regular self-contained version..."
windows true "$APP_NAME-SelfContained$EXE_VERSION"

echo "publishing Linux version..."
linux false "$APP_NAME-linux-x64$EXE_VERSION"

echo "publishing Linux self-contained version..."
linux true "$APP_NAME-SelfContained-linux-x64$EXE_VERSION"

echo "archives in $PUBLISH_PATH:"
ls -1 "$PUBLISH_PATH"
