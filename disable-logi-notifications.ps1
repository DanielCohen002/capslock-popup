<#
.SYNOPSIS
  Force-disables Logi Options+'s Caps Lock / Num Lock / Scroll Lock on-screen popups.

.DESCRIPTION
  Logi Options+ has a "Caps Lock, Num Lock, Scroll Lock and Fn Lock notifications"
  toggle that does not always save. This script removes the Caps Lock (id 2),
  Num Lock (id 3) and Scroll Lock (id 5) entries from Options+'s own
  osd_resources.json, so the agent has nothing to show, then restarts the agent.

  Fn Lock (id 4) and Media Keys (id 6) are left alone.
  Needs administrator rights (it edits Program Files); it asks for them itself.
  A backup of the original file is kept next to it as osd_resources.json.orig.
  A Logi Options+ update may restore the file: just run this script again.

.PARAMETER Restore
  Put the original file back and restart the agent.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File .\disable-logi-notifications.ps1
  powershell -ExecutionPolicy Bypass -File .\disable-logi-notifications.ps1 -Restore
#>
param([switch]$Restore)

$ErrorActionPreference = 'Stop'

# Re-launch elevated if needed.
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    $argList = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSCommandPath`"")
    if ($Restore) { $argList += '-Restore' }
    Start-Process powershell -Verb RunAs -ArgumentList $argList -Wait
    exit
}

$dir    = 'C:\Program Files\LogiOptionsPlus'
$file   = Join-Path $dir 'data\overlay\osd_resources.json'
$backup = "$file.orig"
$agent  = Join-Path $dir 'logioptionsplus_agent.exe'

if (-not (Test-Path $file)) {
    Write-Host "Not found: $file (is Logi Options+ installed?)" -ForegroundColor Red
    Start-Sleep 5; exit 1
}

if ($Restore) {
    if (-not (Test-Path $backup)) { Write-Host "No backup at $backup" -ForegroundColor Red; Start-Sleep 5; exit 1 }
    Copy-Item $backup $file -Force
    Write-Host 'Original file restored.'
} else {
    if (-not (Test-Path $backup)) { Copy-Item $file $backup }   # keep the first original only
    $json = Get-Content $file -Raw | ConvertFrom-Json
    $json.lock_notifications = @($json.lock_notifications | Where-Object { $_.id -notin 2, 3, 5 })
    [IO.File]::WriteAllText($file, ($json | ConvertTo-Json -Depth 20), (New-Object Text.UTF8Encoding $false))
    Write-Host 'Caps / Num / Scroll Lock popups removed from Logi Options+.'
}

# Restart the agent so it re-reads the file (the helper processes can stay running).
Get-Process logioptionsplus_agent -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 2
if (Test-Path $agent) { Start-Process $agent }
Write-Host 'Logi Options+ agent restarted. Done.'
Start-Sleep 3
