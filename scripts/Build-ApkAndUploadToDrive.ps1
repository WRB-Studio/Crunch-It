[CmdletBinding()]
param(
    [string]$UnityPath,
    [int]$VersionCode,
    [string]$DriveDirectory
)

. (Join-Path $PSScriptRoot 'ReleaseCommon.ps1')
if (-not $DriveDirectory -and $script:ReleaseProject.PSObject.Properties['DriveDirectory']) { $DriveDirectory = $script:ReleaseProject.DriveDirectory }
if (-not $DriveDirectory) { throw 'Specify -DriveDirectory or add DriveDirectory to release.config.json.' }
if (-not (Test-Path -LiteralPath $DriveDirectory -PathType Container)) {
    throw "Google Drive folder not found: $DriveDirectory"
}

$build = Invoke-UnityAndroidBuild -Format apk -UnityPath $UnityPath -VersionCode $VersionCode
$actualVersionCode = if ($VersionCode) { $VersionCode } else { $build.Version.VersionCode }
$fileName = "$($script:ReleaseProject.ArtifactName)-$($build.Version.Version)-$actualVersionCode.apk"
$destination = Join-Path $DriveDirectory $fileName
Copy-Item -LiteralPath $build.ArtifactPath -Destination $destination -Force
Write-Host "APK copied to Google Drive: $destination"
