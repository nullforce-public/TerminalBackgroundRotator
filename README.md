# Background Rotator for Windows Terminal

Rotates the background images for Windows Terminal on a set interval with images
retrieved from a folder.

You can configure:
- Which profile to rotate images for
- The folder used for the background images
- The image rotation interval

The application will save a backup of the `profiles.json` for your Windows
Terminal, and restore it on exit so that it can be edited from the Terminal
settings menu.

### For Use With
https://github.com/microsoft/terminal

## Usage

1. Build the application
2. Edit the `appsettings.json`
3. Run `TerminalBackgroundRotator.exe`

Press `CTRL+C` when you are ready to exit. Your previous `profiles.json` should
be restored. If it is not, restore one of the backup copies that the application
has made.

Currently, the application runs only as a .NET Core 3.1 generic host service.
In the future, running it as a Windows service may be possible.

## Config

The application can be configured in `appsettings.json`:
```json
  "Terminal": {
    "ProfilePath": "%LocalAppData%\\Packages\\Microsoft.WindowsTerminal_8wekyb3d8bbwe\\LocalState\\profiles.json",
    "ProfileGuid": "{61c54bbd-c2c6-5271-96e7-009a87ff44bf}",
    "WallpaperDirectory": "%UserProfile%\\Pictures\\Wallpaper",
    "WallpaperIntervalInSeconds": 30
  }
```
