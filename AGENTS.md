# Agent instructions for ColorPicker.Maui

Practical notes accumulated from prior coding sessions. Read before working in this repo to skip re-discovering the quirks.

## Repository layout

| Path | Purpose |
|---|---|
| `ColorPicker/` | The library — the published NuGet package (`ColorPicker.Maui`) |
| `ColorPicker.Core/` | Pure platform-agnostic math (HSL/RGB, polar, unit-square primitives). No MAUI / Skia deps. Multi-targets `netstandard2.0` + `net8.0`. |
| `ColorPicker.Core.Tests/` | xUnit tests for `ColorPicker.Core` (runs on every PR, ubuntu, sub-second) |
| `ColorPickerTestApp/` | MAUI app for manual visual testing |
| `ColorPicker.UITests/` | Appium-driven xUnit UI test suite (~213 tests) |
| `samples/ConsumerSmoke/` | Smoke project that consumes the **packed nupkg**, *not* a ProjectReference |

## Target frameworks

- Library: `net8.0`, `net8.0-android34.0`, `net8.0-windows10.0.19041.0`
- Test app: same three TFMs

## Dependencies & prerequisites

Everything needed to build/test/pack from a clean machine:

| Dependency | Version | Why / notes |
|---|---|---|
| .NET SDK | **8.0.417** | Pinned in `global.json` (`rollForward: latestFeature`) |
| .NET MAUI workloads | — | `dotnet workload install maui-android maui-windows` |
| Android SDK | **API 34** + build-tools 34.0.0 + platform-tools | For `net8.0-android34.0` |
| JDK | **17** (Microsoft OpenJDK) | Required by MAUI Android build; set `JAVA_HOME` |
| Windows 10 SDK | 10.0.19041 | For `net8.0-windows10.0.19041.0` (+ `Microsoft.WindowsAppSDK`) |
| Node.js | **20+** | For Appium (UI tests only) |
| Appium | **2** + `appium-windows-driver` + WinAppDriver 1.2.1 + Windows Developer Mode | UI tests — full setup in [`ColorPicker.UITests/README.md`](ColorPicker.UITests/README.md) |

Key NuGet: `SkiaSharp` 2.88.8, `Microsoft.Maui.Controls` 8.0.x, `Appium.WebDriver` (UITests), `MinVer` 5.0.0.
**MinVer needs full git history + tags** to compute the pack version — clone with full depth (`fetch-depth: 0` in CI).

Environment variables:
- `JAVA_HOME` — JDK 17 path (Android builds)
- `UITEST_APP_PATH` — CI-only; path to the built sample `.exe` the UI tests launch
- `ColorPickerVersion` — nupkg version consumed by `samples/ConsumerSmoke`
- `PROBE_OUT` — dev-only; enables `VisualProbe` scenario dumper (off in CI)

## Build

Quick local build (Windows TFM only):
```powershell
dotnet build ColorPicker\ColorPicker.csproj -c Release -f net8.0-windows10.0.19041.0
```

**Important:** Some warnings only surface on the **Android** TFM (e.g. `CS8765` on `MainActivity.OnCreate(Bundle savedInstanceState)`). Always check Android too if you're hunting warnings:
```powershell
dotnet build ColorPicker\ColorPicker.csproj -c Release -f net8.0-android34.0
```
…or rely on CI's "Build Android" job to surface them.

### ConsumerSmoke pitfall

`samples/ConsumerSmoke/ConsumerSmoke.csproj` references the library via **PackageReference** to the locally packed nupkg, not a ProjectReference. Building it from the solution will fail locally with bogus `CS0234: 'Classes'/'Controls' does not exist in namespace 'ColorPicker'` errors — **ignore those**. Only the CI "Consumer Smoke" job builds it correctly (after Pack NuGet produces the nupkg).

## Tests

UI tests run on Windows via Appium. ~12 minutes for the full 213-test suite.

```powershell
dotnet test ColorPicker.UITests\ColorPicker.UITests.csproj
```

Appium server must be running; CI sets it up via a workflow step.

## CI pipeline

Every PR runs 5 checks (workflow `.github/workflows/build-and-test.yml`):

1. Build Android (~3 min)
2. Build Windows (~3 min)
3. UI Tests (Windows) (~12 min) — the long one
4. Pack NuGet (~2 min)
5. Consumer Smoke (~2 min) — depends on Pack NuGet

Expect ~12-13 minutes for full green.

## Branch protection / merging

`main` is protected:

- PRs required (no direct push)
- **All 5 PR CI checks must pass**, strict (branch must be up-to-date with main before merge)
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
- `ColorPicker/Behaviors/` — touch handling (`TouchBehavior`, `TouchActionEventArgs`)
- `ColorPicker/Platforms/{Android,Windows}/` — touch behavior implementations per platform
