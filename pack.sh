#!/bin/sh

set -e # abort if any command has a non-zero exit code
set -o verbose # prints every line before executing it
set -o xtrace # like verbose but expands variables

GITROOT="$(git rev-parse --show-toplevel)"
RID=${RID:-osx-arm64} # default to apple silicon
PUBLISH_CFG=${PUBLISH_CFG:-Release} # same default as dotnet publish

echo $PUBLISH_CFG
echo $RID

cd "${GITROOT}/artifacts/publish/UlReg.Ui.Desktop/${PUBLISH_CFG}_$RID/"
export VERSION=1.0.0

LIB_EXT=dylib
mkdir -p "Universal Labels.app/Contents/MacOS"
mkdir -p "Universal Labels.app/Contents/Resources"
cp "$GITROOT/UlReg.Ui.Desktop/Info.plist" "Universal Labels.app/Contents/"
cp "$GITROOT/UlReg.Ui.Desktop/icon.icns" "Universal Labels.app/Contents/Resources"
ln ./UlReg.Ui.Desktop ./*.$LIB_EXT "Universal Labels.app/Contents/MacOS/"
mv "Universal Labels.app" "$GITROOT/artifacts"
