# Agent instructions for ColorPicker.Maui

Practical notes accumulated from prior coding sessions. Read before working in this repo to skip re-discovering the quirks.

## Repository layout

| Path | Purpose |
|---|---|
| `ColorPicker/` | The library — the published NuGet package (`ColorPicker.Maui`) |
| `ColorPicker.Core/` | Published `ColorPicker.Maui.Core` package: pure platform-agnostic math (HSL/RGB, polar, unit-square primitives). No MAUI / Skia deps. Multi-targets `netstandard2.0` + `net8.0`. |
| `ColorPicker.Core.Tests/` | xUnit tests for `ColorPicker.Core` (runs on every PR, ubuntu, sub-second) |
| `ColorPickerTestApp/` | MAUI app for manual visual testing |
| `ColorPicker.UITests/` | Appium-driven xUnit UI test suite (~213 tests) |
| `samples/ConsumerSmoke/` | Smoke project that consumes the packed `ColorPicker.Maui` package |
| `samples/CoreConsumerSmoke/` | Smoke project that consumes the packed `ColorPicker.Maui.Core` package for both Core TFMs |
| `samples/PackageCompatibilitySmoke/` | Executable smoke that verifies the Core assembly identity and all type forwarders in the packed MAUI package |

The Core namespaces remain `ColorPicker.Core`; the assembly and NuGet package are
`ColorPicker.Maui.Core` because `ColorPicker.Core` is already owned on nuget.org.
`ColorPicker.dll` contains type forwarders for Core types that were embedded before
the package split.

## Target frameworks

- Library: `net10.0-android`, `net10.0-windows10.0.19041.0` (+ `net10.0-ios`, `net10.0-maccatalyst` on macOS)
- Test app: same TFMs
- `ColorPicker.Maui` is **net10-only**; `ColorPicker.Maui.Core` targets `netstandard2.0` and `net8.0`.
- Test/tooling projects (`ColorPicker.Core`, `*.Tests`, `ColorPicker.UITests`, `tools/IconGen`) stay `net8.0`/`netstandard2.0` and run on the net8 runtime that CI also installs.

## Dependencies & prerequisites

Everything needed to build/test/pack from a clean machine:

| Dependency | Version | Why / notes |
|---|---|---|
| .NET SDK | **10.0.302** | Pinned in `global.json` (`rollForward: latestFeature`) |
| .NET runtime (8.0.x) | — | CI also installs the net8 **runtime** so the `net8.0` test projects run |
| .NET MAUI workloads | — | `dotnet workload install maui-android maui-windows` |
| Android SDK | **API 36** platform + platform-tools | For `net10.0-android`. CI provisions it via `dotnet build -t:InstallAndroidDependencies -f net10.0-android -p:AcceptAndroidSDKLicenses=True` |
| JDK | **17** (Microsoft OpenJDK) | Required by MAUI Android build; set `JAVA_HOME` |
| Windows 10 SDK | 10.0.19041 | For `net10.0-windows10.0.19041.0` |
| Node.js | **20+** | For Appium (UI tests only) |
| Appium | **2** + `appium-windows-driver` + WinAppDriver 1.2.1 + Windows Developer Mode | UI tests — full setup in [`ColorPicker.UITests/README.md`](ColorPicker.UITests/README.md) |

Key NuGet: `SkiaSharp` 4.151.1 + `SkiaSharp.Views.Maui.Controls` 4.151.1, `Microsoft.Maui.Controls` 10.0.20, `Appium.WebDriver` (UITests).
Package versions are supplied by CI. Stable `ColorPicker.Maui` and
`ColorPicker.Maui.Core` versions are independent; main-branch previews use a
coherent same-run version pair.

### .NET 10 / MAUI 10 gotchas (hard-won — don't rediscover)

- **`Microsoft.Maui.Controls` is pinned to the SDK band** (10.0.20 ↔ SDK 10.0.302). Bumping MAUI out of lockstep (even a patch, e.g. 10.0.90) makes restore pull an unpublished runtime pack → `NU1102`. Dependabot ignores `Microsoft.Maui.*` for this reason; bump MAUI **and** the SDK together.
- **`Directory.Build.props` sets `UseMonoRuntime=false` for Windows.** The .NET 10 MAUI Windows head otherwise tries to restore the deprecated `Microsoft.NETCore.App.Runtime.Mono.win-x64` pack (dotnet/maui#27215) → `NU1102`.
- **Restore the Windows app scoped to its TFM.** `dotnet restore ColorPickerTestApp.csproj -r win-x64` on the multi-TFM project applies win-x64 across all TFMs and re-triggers the Mono-win-x64 bug; add `-p:TargetFramework=net10.0-windows10.0.19041.0`.
- **net8 and net10 Android can't be multi-targeted in one build** — the .NET 10 Android workload only recognizes `net10.0-android`; the net8 SDK can't parse `net10.0-*`. (This is why the package went net10-only.)

Environment variables:
- `JAVA_HOME` — JDK 17 path (Android builds)
- `UITEST_APP_PATH` — CI-only; path to the built sample `.exe` the UI tests launch
- `ColorPickerVersion` — package version consumed by an individual smoke project
- `ColorPickerMauiVersion` / `ColorPickerCoreVersion` — package versions supplied during pack
- `ColorPickerCoreDependencyVersion` — exact version or compatible range used when packaging Picker
- `PROBE_OUT` — dev-only; enables `VisualProbe` scenario dumper (off in CI)

## Build

Quick local build (Windows TFM only):
```powershell
dotnet build ColorPicker\ColorPicker.csproj -c Release -f net10.0-windows10.0.19041.0
```

**Important:** Some warnings only surface on the **Android** TFM. Always check Android too if you're hunting warnings (needs the API-36 platform installed):
```powershell
dotnet build ColorPicker\ColorPicker.csproj -c Release -f net10.0-android
```
…or rely on CI's "Build Android" job to surface them.

### ConsumerSmoke pitfall

All three smoke projects reference packages from the local `nupkgs/` feed, not project references, and are intentionally excluded from the solution. Build or run them only after packing both packages and registering that local feed.

Normal development always uses the `ColorPicker` → `ColorPicker.Core`
`ProjectReference`. Package jobs set `UseCorePackageReference=true` so Picker is
compiled against the actual Core nupkg. This prevents a Picker-only release from
silently depending on unreleased Core source.

## Tests

UI tests run on Windows via Appium. ~12 minutes for the full 213-test suite.

```powershell
dotnet test ColorPicker.UITests\ColorPicker.UITests.csproj
```

Appium server must be running; CI sets it up via a workflow step.

## CI pipeline

Every PR runs 6 checks (workflow `.github/workflows/build-and-test.yml`):

1. Core Unit Tests (~30 s) — ubuntu, fast
2. Build Android (~3 min)
3. Build Windows (~3 min)
4. UI Tests (Windows) (~12 min) — the long one
5. Pack NuGet (~2 min)
6. Consumer Smoke (~2 min) — depends on Pack NuGet, so it starts late

**Full green is ~23 min wall-clock**, not the sum of the parts: `Consumer Smoke`
only starts after `Pack NuGet` and re-installs the MAUI workloads, so it finishes
several minutes *after* the ~12-min UI Tests. Size any CI-watch/poll loop to **~25 min**.

Every merge to `main` publishes both packages to GitHub Packages with the same
`0.0.0-preview.<run>` version and an exact Picker → Core dependency. Workflow
retries reuse the same version.
Stable nuget.org releases are started from **Actions → Release → Run workflow**,
where the target is `picker`, `core`, or `both` and stable versions are entered
independently.

## Branch protection / merging

`main` is protected:

- PRs required (no direct push)
- **All 6 PR CI checks must pass**, strict (branch must be up-to-date with main before merge)
- 0 required reviews — solo project, so auto-merge from the author can fire as soon as CI is green
- Force pushes / deletions disabled
- Conversation resolution required
- Admins **not** enforced — `gh pr merge --admin` still works as an emergency exit if you absolutely have to ship something past CI

Normal merge flow:

```bash
gh pr merge N --squash --delete-branch --auto
```

`--auto` queues the merge so it fires automatically when all required checks pass. Convention is **squash** merges.

Emergency / docs-only merges that don't need CI gating:

```bash
gh pr merge N --admin --squash --delete-branch
```

Use sparingly — defeats the purpose of the gate.

## Formatting & style

- Style is mainstream .NET (Microsoft / Roslyn defaults). See `.editorconfig`.
- File-scoped namespaces everywhere: `namespace Foo;`
- No spaces inside parens/brackets: `Foo(x, y)`, `arr[i]`, `if (cond)`
- `using` directives go **outside** the namespace, no separated import groups

Run the formatter:
```powershell
dotnet format ColorPicker\ColorPicker.csproj
```

**Limitation:** `dotnet format whitespace` only fixes **single-line** paren/bracket spacing. Multi-line method calls like:

```csharp
new Foo( a,
         b )
```

…will not be touched. For broad cleanup, a regex pass is needed:

```powershell
git ls-files '*.cs' | ForEach-Object {
  $t = Get-Content $_ -Raw
  $t = $t -replace '\( (?=\S)', '('
  $t = $t -replace '(?<=\S) \)', ')'
  $t = $t -replace '\[ (?=\S)', '['
  $t = $t -replace '(?<=\S) \]', ']'
  Set-Content $_ -Value $t -NoNewline
}
```

The `\S` lookarounds keep empty `()` / `[]` intact and don't touch existing tight pairs.

## Imaging

Uses **SkiaSharp**, not ImageSharp (replaced in checkpoint 038). When working with bitmap rendering or tests that capture pixels, reach for `SKBitmap` / `SKSurface`, not `Image<Rgba32>`.

## Common warning fixes

- **CS8767** (Nullability mismatch on `IValueConverter` implementations): change `object` → `object?` on `value`, `parameter`, and the return type. Keep `Type targetType` and `CultureInfo culture` non-nullable. Add `!` on the cast sites if existing code already assumed non-null.
- **CS8765** in `MainActivity.OnCreate(Bundle savedInstanceState)`: change to `Bundle? savedInstanceState`. Android-only.
- **XML doc comment errors** (`Expected end tag…`): escape `<` / `>` in doc strings — use `&lt;` / `&gt;` or wrap in `<c>…</c>`.

## Conventions in the test suite

- **Tier-named test groups** (Tier 1 = LayoutSmoke, Tier 2 = feature matrix, Tier 3 = pixel-diff, Tier 4 = container sizing, etc.).
- xUnit, not NUnit.
- Page Object pattern under `ColorPicker.UITests/PageObjects/`.
- `VisualProbe` is a developer-only scenario dumper, gated on `PROBE_OUT` env var so it doesn't run in CI.

## Where things live

- `ColorPicker/BaseClasses/` — `ColorPickerBase`, `SkiaPickerBase`, `SliderBase`, `SliderStack`, `SliderStackWithAlpha`
- `ColorPicker/Controls/` — `ColorWheel`, `ColorTriangle`, `ColorDisc`, `HslSlider`, `RgbSlider`, `AlphaSlider`, `LuminositySlider`, `DelegateSlider`
- `ColorPicker/Rendering/` — renderer contracts, semantic drawing contexts, gradients, and bundled renderer implementations
- `ColorPicker/Behaviors/` — touch handling (`TouchBehavior`, `TouchActionEventArgs`)
- `ColorPicker/Platforms/{Android,Windows}/` — touch behavior implementations per platform
