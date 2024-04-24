# Changelog

## v0.2.2 prerelease | 24-04-2024

### Additions

- (Dev) Started work on API to fetch higher quality album art.

### Fixes

- Fixed timestamp showing up when song is paused.
- Fixed error on fetching album art from iTunes API.

## v0.2.1 prerelease | 23-04-2024

### Additions

- Added Dynamic album art fetched from the Apple Music API (might break at times).
- Added Mica design to the window to match Windows 11.

### Fixes / Improvements

- Removed the console popping up when running the app.
- Fixed timestamp showing up when the RPC was in idle mode.
- Improved player detection. The RPC should now not show up for media being played in other apps.
-  (Dev) Upgraded .NET to version 8

## v0.1.0 prerelease | 02-10-2023

### Additions

- Added run on startup feature.
- (Dev) Upgrade to .NET 7 and latest Avalonia.FuncUI.
- Added timestamp to presence.

### Fixes

- Fixed broken checkboxes.
- Fixed broken stop button (used to not end presence on Discord).

## v0.0.3 prerelease | 22-7-2022

### Additions

- Add LiteDB for local storage

### Deletions

- Remove settings.json

### Fixes

- Fix app not starting due to absence of settings.json file

## v0.0.2 prerelease | 17-7-2022

### Additions

- Added switches for run on startup and hide on start tasks ( Both are not functional as of now )
- Added idle presence when MediaPlayer is closed

### Deletions

- Temporarily remove timestamps as they do not work as intended
