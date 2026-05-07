# PartyIcons Handover

Updated: 2026-05-06

## Current Status

- Working directory: `E:\git\PartyIcons`
- Primary remote: `origin = https://github.com/anmili2022/xivPartyIcons`
- Main branch: `main`
- Current `main` commit at handover time: `fec5678`
- Latest retained release tag at handover time: `1.2.3.7`
- Failed release tag `1.2.3.5` was cleaned up after the packaging fix
- Local debug build status: `dotnet build PartyIcons.sln -c Debug -p:Platform=x64` passes

Expected debug output files:

- `Output\Debug\PartyIcons.dll`
- `Output\Debug\PartyIcons.json`
- `Output\Debug\icon.png`
- `Output\Debug\images\icon.png`

## What Changed In This Round

This maintenance pass repaired the build and release path so the fork can be built locally and published from GitHub Actions without relying on stale `bin\` output.

Completed changes:

1. Build output path
   - `PartyIcons\PartyIcons.csproj` sets `OutDir` to `$(SolutionDir)Output\$(Configuration)\`
   - Debug and release output now land in `Output\Debug\` and `Output\Release\`

2. Plugin manifest copy
   - `PartyIcons.json` is copied to the output directory with `CopyToOutputDirectory=PreserveNewest`
   - This keeps local dev output directly loadable by Dalamud

3. Main UI callback registration
   - `PartyIcons\UI\WindowManager.cs` registers `UiBuilder.OpenMainUi += _settingsWindow.Toggle`
   - The matching unregister stays in `Dispose`

4. API compatibility sync
   - Runtime and UI compatibility code was aligned to the current Dalamud CN API usage
   - Main files touched in that repair:
     - `PartyIcons\Runtime\ChatNameUpdater.cs`
     - `PartyIcons\Runtime\ViewModeSetter.cs`
     - `PartyIcons\Runtime\PartyListHUDUpdater.cs`
     - `PartyIcons\Runtime\UpdateContext.cs`
     - `PartyIcons\Runtime\RoleTracker.cs`
     - `PartyIcons\UI\Utils\ImGuiExt.cs`

5. Release workflow
   - `.github\workflows\release.yml` builds the solution in `Release|x64`
   - The workflow archives `Output\Release\*` into `PartyIcons.zip`
   - Tag pushes matching `*.*.*.*` create GitHub releases automatically

6. Packager output-path fix
   - Added `PartyIcons\DalamudPackager.targets`
   - This overrides the default `DalamudPackager` behavior so it packages from `$(TargetDir)` instead of the stale `$(OutputPath)` under `bin\x64\...`
   - This fix is required for clean GitHub runners, where `bin\x64\Release\PartyIcons.dll` does not exist

7. Repo metadata and versioning
   - `PackageProjectUrl`, `RepoUrl`, and `IconUrl` point to `anmili2022/xivPartyIcons`
   - `PartyIcons\PartyIcons.csproj` currently carries version `1.2.3.7`

## Build

Run from the repo root:

```powershell
dotnet build PartyIcons.sln -c Debug -p:Platform=x64
```

Debug output:

```text
E:\git\PartyIcons\Output\Debug\
```

For a local release-style build:

```powershell
$dalamudPath = "$env:AppData\XIVLauncherCN\addon\Hooks\dev\"
dotnet restore .\PartyIcons.sln -p:Platform=x64
dotnet build .\PartyIcons.sln --configuration Release --no-restore --nologo -p:Platform=x64 -p:DalamudLibPath="$dalamudPath" -p:Version=1.2.3.7 -p:FileVersion=1.2.3.7 -p:AssemblyVersion=1.2.3.7
```

Expected release output:

```text
E:\git\PartyIcons\Output\Release\
E:\git\PartyIcons\Output\Release\PartyIcons\latest.zip
```

## Release Workflow

Workflow file:

```text
.github\workflows\release.yml
```

Behavior:

1. Trigger automatically when pushing a tag matching `*.*.*.*`, for example `1.2.3.7`
2. Or trigger manually in GitHub Actions using `workflow_dispatch` and a tag input
3. Download Dalamud CN dependencies on the Windows runner
4. Restore and build `PartyIcons.sln` in `Release|x64`
5. Archive `Output\Release\*` into `PartyIcons.zip`
6. Create a GitHub Release and upload the zip asset

Implementation notes:

- The workflow intentionally builds the solution with `Platform=x64`
- `PartyIcons\DalamudPackager.targets` is not optional while `OutDir` points outside the project `bin\` folder
- Building `PartyIcons\PartyIcons.csproj` directly can use a different output root than the solution build, so keep the release workflow aligned with the solution unless there is a deliberate reason to change it

Recommended release flow:

```powershell
git checkout main
git pull --ff-only
git tag 1.2.3.7
git push origin main
git push origin 1.2.3.7
```

## Quick Release

Use this when you already have a tested fix and want to publish with minimal ceremony.

Release rules:

- Version source: `PartyIcons\PartyIcons.csproj`
- Release trigger: push a tag matching `*.*.*.*`
- Current version example: `1.2.3.7`
- On this machine, global Git config currently has `tag.gpgSign=true`, so plain `git tag ...` may hang if GPG is not ready

Fast path:

1. Update `<Version>` in `PartyIcons\PartyIcons.csproj` to the next release number
2. Build locally:

```powershell
dotnet build .\PartyIcons.sln -c Release -p:Platform=x64
```

3. Commit only the intended release files:

```powershell
git add PartyIcons/PartyIcons.csproj
git add PartyIcons/Runtime/NameplateUpdater.cs
git commit -m "Describe the release"
```

4. Push the branch:

```powershell
git push origin main
```

5. Create and push the release tag:

```powershell
git -c tag.gpgSign=false tag 1.2.3.8
git push origin 1.2.3.8
```

6. Verify GitHub Actions created the release:
   - Workflow: `.github\workflows\release.yml`
   - Release page: `https://github.com/anmili2022/xivPartyIcons/releases`

If the tag was created with the wrong number:

```powershell
git push origin :refs/tags/1.2.3.8
git tag -d 1.2.3.8
```

If you need to clean a failed release:

```powershell
git push origin :refs/tags/1.2.3.7
git tag -d 1.2.3.7
```

You may also need to delete the matching GitHub release entry if the workflow already created one.

## Load And Verify

When validating in Dalamud dev plugin mode, check these items first:

1. `PartyIcons.dll` and `PartyIcons.json` both exist in `Output\Debug\`
2. The plugin was reloaded after replacing files, not only copied over
3. The old Dalamud warning about a missing main UI callback is gone
4. Opening the plugin main UI works without relying only on the config entry point

If the warning still appears, the most likely cause is that Dalamud is still loading an older DLL.

## Reference Source

The main reference repo used during the API/build repair was:

```text
E:\git\bubu\PartyIcons
```

If future Dalamud updates break the build again, compare the runtime and UI compatibility layers in that repo first.

## Notes

- This document is for maintainers, not end-user plugin documentation
- `README.md` links here for release and maintenance instructions
- Before pushing a new release tag, prefer validating both a local debug build and a clean release-style build
- If `git tag` appears to hang on this workstation, use `git -c tag.gpgSign=false tag ...` unless GPG signing is intentionally required
