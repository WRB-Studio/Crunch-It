[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$KeystorePath,
    [Parameter(Mandatory)][string]$KeyAlias,
    [Parameter(Mandatory)][string]$ServiceAccountJsonPath
)

. (Join-Path $PSScriptRoot 'ReleaseCommon.ps1')

if (-not (Test-Path -LiteralPath $KeystorePath -PathType Leaf)) { throw "Keystore not found: $KeystorePath" }
if (-not (Test-Path -LiteralPath $ServiceAccountJsonPath -PathType Leaf)) { throw "Service-account JSON not found: $ServiceAccountJsonPath" }

$keystorePassword = Read-Host 'Keystore password' -AsSecureString
$keyAliasPassword = Read-Host 'Key-alias password' -AsSecureString
$configDirectory = Split-Path -Parent $script:ReleaseConfigPath
New-Item -ItemType Directory -Force -Path $configDirectory | Out-Null

[PSCustomObject]@{
    KeystorePath = $KeystorePath
    KeyAlias = $KeyAlias
    KeystorePassword = $keystorePassword
    KeyAliasPassword = $keyAliasPassword
    ServiceAccountJsonPath = $ServiceAccountJsonPath
} | Export-Clixml -LiteralPath $script:ReleaseConfigPath -Force

Write-Host "Release configuration saved for this Windows account: $script:ReleaseConfigPath"
