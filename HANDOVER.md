# PartyIcons Handover

Updated: 2026-05-06

## Current Status

- Working directory: `E:\git\PartyIcons`
- Primary remote: `origin = https://github.com/anmili2022/xivPartyIcons`
- Default push target: `origin`
- Main branch: `main`
- Current published main commit at handover time: `753e01d`
- Debug build status: `dotnet build PartyIcons.sln -c Debug` passes
- Build result: `0 warnings / 0 errors`
- Debug output directory: `Output\Debug\`

Expected debug output files:

- `Output\Debug\PartyIcons.dll`
- `Output\Debug\PartyIcons.json`
- `Output\Debug\icon.png`
- `Output\Debug\images\icon.png`

## What Was Changed

This round of work focused on making the plugin build cleanly again, making the local output directly loadable by Dalamud, and wiring the repo for automatic GitHub releases.

Completed changes:

1. Build output path
   - Added `OutDir` in `PartyIcons\PartyIcons.csproj`
   - Debug and release builds now go to `Output\Debug\` and `Output\Release\`

2. Plugin manifest copy
   - Added `CopyToOutputDirectory=PreserveNewest` for `PartyIcons.json`
   - This ensures the built output contains both the DLL and plugin manifest

3. Main UI callback registration
   - Added `UiBuilder.OpenMainUi += _settingsWindow.Toggle` in `PartyIcons\UI\WindowManager.cs`
   - Kept config UI registration and proper unregistration in `Dispose`
   - This addresses the Dalamud validation warning about a missing main UI callback

4. API compatibility sync
   - Synced the runtime/UI compatibility layer to current Dalamud CN API usage
   - Main areas updated:
     - `PartyIcons\Runtime\ChatNameUpdater.cs`
     - `PartyIcons\Runtime\ViewModeSetter.cs`
     - `PartyIcons\Runtime\PartyListHUDUpdater.cs`
     - `PartyIcons\Runtime\UpdateContext.cs`
     - `PartyIcons\Runtime\RoleTracker.cs`
     - `PartyIcons\UI\Utils\ImGuiExt.cs`

5. Additional cleanup
   - `PartyIcons\UI\Utils\FlashingText.cs`: aligned `PushColor` return type with current API
   - `PartyIcons\UI\Settings\ChatNameTab.cs`: removed unused exception variable
   - `PartyIcons\Configuration\Settings.cs`: added null fallback for legacy config deserialization
   - `PartyIcons\Runtime\RoleTracker.cs`: fixed a malformed verbose log string

6. Repo metadata updated to the fork
   - `PartyIcons\PartyIcons.csproj`: `PackageProjectUrl` now points to `https://github.com/anmili2022/xivPartyIcons`
   - `PartyIcons\PartyIcons.json`: `RepoUrl` and `IconUrl` now point to the same fork

7. GitHub release workflow added
   - Added `.github\workflows\release.yml`
   - Supports automatic release on tag push and manual release via `workflow_dispatch`

## Build

Run from the repo root:

```powershell
dotnet build PartyIcons.sln -c Debug
```

Debug output:

```text
E:\git\PartyIcons\Output\Debug\
```

## Release Workflow

Workflow file:

```text
.github\workflows\release.yml
```

Behavior:

1. Trigger automatically when pushing a tag matching `*.*.*.*`, for example `1.2.3.5`
2. Or trigger manually in GitHub Actions using `workflow_dispatch` and a tag input
3. Download Dalamud CN dependencies on the Windows runner
4. Build `PartyIcons.sln` in `Release|x64`
5. Archive `Output\Release\*` into `PartyIcons.zip`
6. Create a GitHub Release and upload the zip asset

Implementation note:

- The workflow intentionally builds the solution with `Platform=x64`
- Building `PartyIcons\PartyIcons.csproj` directly can produce a different output path and break release packaging

Recommended release flow:

```powershell
git checkout main
git pull
git tag 1.2.3.5
git push origin main --tags
```

## Load And Verify

When validating in Dalamud dev plugin mode, check these items first:

1. `PartyIcons.dll` and `PartyIcons.json` both exist in `Output\Debug\`
2. The plugin was reloaded after replacing files, not only copied over
3. The old Dalamud warning about missing main UI callback is gone
4. Opening the plugin main UI works without relying only on the config entry point

If the warning still appears, the most likely cause is that Dalamud is still loading an older DLL.

## Reference Source

The repair work was based mainly on the local reference repo:

```text
E:\git\bubu\PartyIcons
```

If future API updates break the build again, compare the runtime and UI compatibility layers in that repo first.

## Notes

- This document is intended as a maintainer handover, not end-user documentation.
- At the time of this update, `main` already contains the build/output fixes and release workflow.
