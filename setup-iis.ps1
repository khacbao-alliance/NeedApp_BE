$ErrorActionPreference = 'Stop'
Start-Transcript -Path 'e:\HungNM\NeedApp_BE\setup-iis.log' -Force | Out-Null
trap { $_ | Out-String | Write-Host; Stop-Transcript | Out-Null; exit 1 }
& "$env:windir\system32\inetsrv\appcmd.exe" set config -section:system.webServer/proxy /enabled:"True" /commit:apphost | Out-Null
New-Item -ItemType Directory -Force -Path 'C:\inetpub\needapp' | Out-Null
Copy-Item -Path 'e:\HungNM\NeedApp_BE\web.config' -Destination 'C:\inetpub\needapp\web.config' -Force
Import-Module WebAdministration
if (Get-Website -Name 'NeedApp' -ErrorAction SilentlyContinue) {
    Remove-Website -Name 'NeedApp'
}
New-Website -Name 'NeedApp' -PhysicalPath 'C:\inetpub\needapp' -Port 8090 -Force | Out-Null
Start-Website -Name 'NeedApp'
Get-Website -Name 'NeedApp' | Format-List Name, State, PhysicalPath, Bindings
& "$env:windir\system32\inetsrv\appcmd.exe" list config -section:system.webServer/proxy
Stop-Transcript | Out-Null
