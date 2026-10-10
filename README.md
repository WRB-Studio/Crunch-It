<img align="center" src="https://user-images.githubusercontent.com/32596430/155371875-4cfd57bc-1807-467a-ae79-a998f4297d69.png" width="400" height="150" /> <img align="center" src="https://user-images.githubusercontent.com/32596430/155372205-bb77a3da-576c-4875-b3e2-a533134e1ad9.png" width="80" height="80" />
<br>
<br>
<p>
A mini android game where you kill cute little crunchies with your fingers.<br> Tap them to kill them.
</p>

Available on Google Play <a href="https://play.google.com/store/apps/details?id=com.WRBStudio.CrunchIt&pli=1">Crunch-It</a>.
<br>
<br>

<p float="left">
  <img src="https://user-images.githubusercontent.com/32596430/155371132-5994cfed-8610-4ac4-9c10-383c49c64eeb.png" width="230" height="410" />
  <img src="https://user-images.githubusercontent.com/32596430/155371157-9c976016-59d0-4c13-b2dc-8d465af85fc4.png" width="230" height="410" /> 
</p>

## Unity Android Release Tools

Integrated from [WRB-Studio/unity-android-release-tools](https://github.com/WRB-Studio/unity-android-release-tools) at commit `8d9fce859fa9a606a621acd524665e109e7b8e28` (fetched `origin/main`, 2026-10-10).
For future updates, follow the [update guide at this revision](https://github.com/WRB-Studio/unity-android-release-tools/blob/8d9fce859fa9a606a621acd524665e109e7b8e28/UPDATING.md): use the latest fetched `origin/main` by default and update all PowerShell/Ruby scripts and both Editor files together, preserving this project's configuration, local secret locations and existing `.meta` GUIDs. Record the actual source revision after verification. Scripts and helper use build protocol 1 and validate the receipt's protocol and artifact format before export or upload.
Project configuration is in `scripts/release.config.json`; builds and logs go to `Builds/Android`.
Requires Unity Android Build Support (SDK/NDK/OpenJDK), Ruby and Fastlane.

Configure credentials once with `scripts/Set-ReleaseSecrets.ps1 -KeystorePath '<existing-upload-keystore>' -KeyAlias '<existing-alias>' -ServiceAccountJsonPath '<google-play-key.json>'`.
Passwords are entered through a hidden prompt and stored encrypted for the current Windows account outside the repository.
Keep key files outside this project or in the ignored `Secrets/` directory.

Reuse an existing Google Cloud project and service account where available. The Google Play Android Developer API must be enabled in that project, and the service account needs access to Crunch It in Play Console. For internal testing, grant app read access and test-track release access. Production uploads require the separate production-release permission. Setup references: [Google Play Developer API](https://developers.google.com/android-publisher/getting_started), [Play Console permissions](https://support.google.com/googleplay/android-developer/answer/9844686).

Reuse the existing shared service account (WRB Studio Unity Play Uploader) for additional games, grant access per app, and keep each game's upload keystore and alias in its own local release configuration. Account identity, key-file paths and signing credentials are stored locally outside Git.

```powershell
# Local checks without a build or upload
./scripts/Test-ReleaseWorkflow.ps1
# Check Google Play access and the next available version code
./scripts/Build-AabAndSubmitToPlay.ps1 -CheckOnly
# Build a signed APK (save and close this project's Unity Editor first)
./scripts/Build-Apk.ps1
# Build and upload to internal testing
./scripts/Build-AabAndSubmitToPlay.ps1 -Track internal
# Build and submit a production release
./scripts/Build-AabAndSubmitToPlay.ps1 -ConfirmProduction
```

Use the existing Crunch It upload key. Uploads are started explicitly by these commands; pushing to Git does not publish a release.

### Local Drive export and offline checks

The shared workflow can export signed APKs or AABs into an existing local Drive folder. Configure the destination outside Git with `Set-DriveExportDirectory.ps1`; `release.config.json` and its existing `SecretsKey` remain unchanged. Signing credentials and Play-key locations remain in the existing encrypted local configuration.

```powershell
# Configure an existing local destination; does not build or copy
./scripts/Set-DriveExportDirectory.ps1 -DriveDirectory '<existing-local-folder>'
# Check the destination and build helper locally; no build, copy or network
./scripts/Build-AndroidAndExportToDrive.ps1 -CheckOnly
# Compile the helper against this project's Unity version without starting Unity
./scripts/Test-UnityHelperCompilation.ps1 -UnityPath '<installed-Editor/Unity.exe>'
```

An explicitly requested export uses `Build-AndroidAndExportToDrive.ps1 -Format apk` or `-Format aab`; the previous `Build-ApkAndUploadToDrive.ps1` command remains an APK wrapper. Existing exports require `-Force` to replace. These commands verify the local copy by SHA-256, but do not confirm cloud synchronization. Copying into a synced folder can trigger an upload; when transfers are prohibited, use only `-CheckOnly` or `Test-ReleaseWorkflow.ps1`. The test suite uses temporary fixtures and simulated builds/Play commands, without real credentials or uploads.

For rollback, restore the tool scripts and Unity helper together from the last working project commit while retaining unrelated game changes, project configuration and local credentials.

Update verification (2026-10-10): the offline suite passed under Windows PowerShell 5.1 and PowerShell 7, including simulated device installation/launch and menu dispatch. Both Editor files compiled against Unity 6000.3.25f1 with warnings treated as errors; all 18 menu actions registered in a disposable Unity test project. The existing Drive destination passed the local CheckOnly command. Project configuration, existing `.meta` files, game settings, approved metadata and local credentials were retained. No real Android build, device installation, Play API check, export or publication was performed.

### Unity menu and USB installation

Both Editor files are installed: `Assets/Editor/UnityAndroidBuild.cs` and `Assets/Editor/UnityAndroidReleaseMenu.cs`. Open **Tools → Unity Android Release Tools** for APK/AAB builds, phone installation, Drive exports, Play releases, setup, tests and logs. Production submission requires confirmation in the Editor. Menu builds use a dedicated isolated copy so the source Editor can stay open; save scenes and assets first. Allow at least 25 GB free space on the build cache/output volumes. Use a new empty cache folder or one already marked by the tool for this project.

```powershell
# Local signed AAB build, without upload
./scripts/Build-Aab.ps1
# Optional isolated signed build while the source Editor stays open
./scripts/Build-Apk.ps1 -BuildRoot '<dedicated-empty-cache-folder>'
# Configure local Editor/ADB/cache paths without building or installing
./scripts/Set-AndroidDeviceBuildSettings.ps1 -UnityPath '<installed-Editor/Unity.exe>' -BuildRoot '<dedicated-empty-cache-folder>'
# Check an attached authorized Android device without building/installing
./scripts/Build-ApkAndInstall.ps1 -CheckOnly
# Only when installation is requested: build, install and launch
./scripts/Build-ApkAndInstall.ps1 -Development
```

Device builds use the existing package identity and leave the source project's version/signing settings unchanged. They use debug signing by default; `-ReleaseSigning` uses the existing release credentials. With multiple devices choose `-DeviceSerial`, or use the menu's device picker. Installation preserves app data and launches the verified app; `-NoLaunch` disables launching for CLI use. Signature conflicts stop the operation; `-ReplaceExistingApp` permits uninstalling the target app and deleting its local data only when explicitly requested. Neither device build variant uploads to Play or Drive.

Device/cache settings are local: `Builds/Android/device-build.json` takes precedence over `%LOCALAPPDATA%/UnityAndroidRelease/<SecretsKey>/device-build.json`. Both are ignored by Git; device serial numbers are not saved. Menu registration and fixture tests do not replace a manual gameplay, USB or cancellation test in the actual project.

### Store descriptions and release notes

The approved German and English texts are stored in `release/play-metadata.json`. Update and review this file before each release. It contains public texts only; credentials remain in the local encrypted configuration.

```powershell
# Preview and validate texts locally; no network, build or upload
./scripts/Submit-PlayMetadata.ps1 -MetadataFile release/play-metadata.json -VersionCode 20 -CheckOnly
# Preview the next versioncode online without building or uploading
./scripts/Build-AabAndSubmitToPlay.ps1 -MetadataFile release/play-metadata.json -CheckOnly
# Build and submit a NEW production release with approved texts
./scripts/Build-AabAndSubmitToPlay.ps1 -MetadataFile release/play-metadata.json -ConfirmMetadata -ConfirmProduction
# Update texts for an EXISTING release without rebuilding (supply its actual versioncode)
./scripts/Submit-PlayMetadata.ps1 -MetadataFile release/play-metadata.json -VersionCode 20 -ConfirmMetadata -ConfirmProduction
```

Omitted description fields remain unchanged. Images and screenshots are skipped. When changing release notes, include every desired language: Fastlane replaces the target release's notes. Every listed language must then have `releaseNotes`. Text limits and package identity are checked before upload. `-ReleaseStatus draft` and `-ChangesNotSentForReview` on the build command allow a draft/deferred submission; deferred changes must be submitted in Play Console.

The shared service account has Crunch It production-release and store-presence permissions. Both API write permissions were validated on 2026-10-02 by resubmitting unchanged existing data in a temporary edit, validating it, and deleting it without a commit. Read-only `-CheckOnly` alone does not prove write permissions. A successful API commit still requires Google's review; automatic publication depends on Play Console publishing settings. Do not run the production commands merely to test the integration.
