# CapsPopup

A tiny Windows tray app that shows a clean on-screen popup at the top of the screen when you press Caps Lock, Num Lock or Scroll Lock.

- Light rounded card, top center, fades out after about a second
- Strike-through on the icon when the key turns off
- Never takes focus; clicks pass through it
- Sharp on high-DPI screens
- No tray icon, nothing in the taskbar. Starts with Windows by default (it has to be running to work)

## Install

Download `CapsPopup.exe` from the [Releases](../../releases) page and run it. It adds itself to Windows startup on first run. No installer, no dependencies (uses the .NET Framework that ships with Windows).

There is no tray icon. To stop it, end `CapsPopup` in Task Manager. To stop it starting with Windows, turn it off in Task Manager > Startup apps (or Settings > Apps > Startup).

## Build

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -target:winexe -out:CapsPopup.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll CapsPopup.cs
```

## Turn off Logitech's popup first

If you use a Logitech keyboard or mouse, **Logi Options+ has its own Caps Lock / Num Lock notification, and it has to be turned off** or you will see two popups (Logi's at the bottom of the screen, CapsPopup at the top). Logi has no setting to move its popup, only on/off.

In Logi Options+, open your keyboard's settings and turn off the on-screen notification for Caps Lock and Num Lock. The same goes for any other tool that shows its own lock-key popup (for example ASUS or Dell utilities).

### If the Options+ toggle does nothing

On some setups the Options+ "Caps Lock, Num Lock, Scroll Lock and Fn Lock notifications" toggle never saves (this can happen even with a non-Logitech keyboard when a Logitech receiver is plugged in). `disable-logi-notifications.ps1` forces the popups off instead:

```
powershell -ExecutionPolicy Bypass -File .\disable-logi-notifications.ps1
```

It asks for administrator rights, removes the Caps Lock, Num Lock and Scroll Lock entries from `C:\Program Files\LogiOptionsPlus\data\overlay\osd_resources.json`, saves the original as `osd_resources.json.orig` next to it, and restarts the Options+ agent. Fn Lock and Media Keys notifications are untouched. Add `-Restore` to put the original back.

A Logi Options+ update can overwrite that file; if the popup comes back, run the script again.
