# CapsPopup

A tiny Windows tray app that shows a clean on-screen popup at the top of the screen when you press Caps Lock, Num Lock or Scroll Lock.

- Light rounded card, top center, fades out after about a second
- Strike-through on the icon when the key turns off
- Never takes focus; clicks pass through it
- Sharp on high-DPI screens
- Starts with Windows by default (it has to be running to work); right-click the tray icon and untick "Start with Windows" to turn that off, or choose "Exit"

## Install

Download `CapsPopup.exe` from the [Releases](../../releases) page and run it. No installer, no dependencies (uses the .NET Framework that ships with Windows).

## Build

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -target:winexe -out:CapsPopup.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll CapsPopup.cs
```

## Turn off Logitech's popup first

If you use a Logitech keyboard or mouse, **Logi Options+ has its own Caps Lock / Num Lock notification, and it has to be turned off** or you will see two popups (Logi's at the bottom of the screen, CapsPopup at the top). Logi has no setting to move its popup, only on/off.

In Logi Options+, open your keyboard's settings and turn off the on-screen notification for Caps Lock and Num Lock. The same goes for any other tool that shows its own lock-key popup (for example ASUS or Dell utilities).
