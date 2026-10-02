[CmdletBinding()]
param(
    [string]$UnityPath,
    [int]$VersionCode
)

. (Join-Path $PSScriptRoot 'ReleaseCommon.ps1')
Invoke-UnityAndroidBuild -Format apk -UnityPath $UnityPath -VersionCode $VersionCode
