# Changelog

## v0.1.0 prerelease | 02-10-2023

### Additions

- Add run on startup feature.
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
