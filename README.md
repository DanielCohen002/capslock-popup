# CapsPopup

A tiny Windows tray app that shows a clean on-screen popup at the top of the screen when you press Caps Lock, Num Lock or Scroll Lock.

- Light rounded card, top center, fades out after about a second
- Strike-through on the icon when the key turns off
- Never takes focus; clicks pass through it
- Sharp on high-DPI screens
- Right-click the tray icon for "Start with Windows" and "Exit"

## Install

Download `CapsPopup.exe` from the [Releases](../../releases) page and run it. No installer, no dependencies (uses the .NET Framework that ships with Windows).

## Build

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -target:winexe -out:CapsPopup.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll CapsPopup.cs
```

## Notes

If another tool (for example Logitech Options+) already shows its own lock-key popup, turn that one off to avoid two popups.
