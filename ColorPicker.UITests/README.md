# ColorPicker.UITests

Cross-platform UI tests for **ColorPicker.Maui**, driven by **Appium 2** and
**xUnit**. The same C# tests will run against Android/iOS once those drivers are
wired up; today the suite targets Windows.

## What's covered

- **SmokeTests** — app launches and renders the wheel; readouts populate.
- **SettingsToggleTests** — switches change state; the triangle/wheel swap works.
- **ColorWheelInteractionTests** — drag gestures on the wheel update `SelectedColor`.
- **SliderTests** — HSL & RGB sliders update `SelectedColor`.

## Architecture

- `Infrastructure/AppiumServerFixture.cs` — collection fixture; reuses an
  existing Appium server on `127.0.0.1:4723` or spawns one for the run.
- `Infrastructure/AppFixture.cs` — class fixture; launches the sample
  `ColorPickerTestApp.exe`, waits for the top-level window, and attaches a
  `WindowsDriver` via the `appTopLevelWindow` capability. (Letting WAD launch
  the app itself is unreliable for MAUI WinUI3, whose main window appears
  asynchronously.)
- `PageObjects/MainPage.cs` — element accessors keyed by `AutomationId` plus
  pointer-gesture helpers (`TapInside`, `DragInside`, `…Square` variants for
  circular controls).
- `Tests/` — `[Fact]`-style xUnit tests organized by feature.

## Why a host `Border` around each SkiaSharp control

`controls:ColorWheel`, `ColorTriangle`, `HSLSliders`, `RGBSliders` are rendered
by SkiaSharp into a single GPU surface. UIA sees the surface as opaque and
neither the controls nor any of their inner widgets appear in the accessibility
tree. To make them addressable for tests, each is wrapped in a MAUI `Border`
with an `AutomationId` and a non-empty `SemanticProperties.Description` (without
the description, MAUI Windows flattens layout-only containers out of the UIA
tree). Tests interact with the host Border using coordinate-based pointer
actions; `…Square` helpers compute the centered square inside the (often wider)
host so taps land on the visible disc.

## Running locally (Windows)

Prerequisites:

```powershell
# .NET 8 + MAUI workloads
dotnet workload install maui-windows

# Node 20+, Appium 2, Windows driver
npm install -g appium@2
appium driver install --source=npm appium-windows-driver

# WinAppDriver (Microsoft installer)
#   https://github.com/microsoft/WinAppDriver/releases
#   Requires Windows Developer Mode:
#     reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" `
#       /v AllowDevelopmentWithoutDevLicense /t REG_DWORD /d 1 /f
```

Run:

```powershell
# Build the sample app once.
dotnet build ColorPickerTestApp/ColorPickerTestApp.csproj `
  -c Release -f net8.0-windows10.0.19041.0 -r win-x64

# Start Appium server (in another terminal).
appium

# Run the suite.
dotnet test ColorPicker.UITests/ColorPicker.UITests.csproj
```

## Running in CI

See `.github/workflows/build.yml` → job `ui-tests-windows`. It installs all the
prerequisites, starts an Appium server, builds the sample, and runs the tests
against the Release output. Test results and the Appium log are uploaded as the
`ui-test-results` artifact on every run (success or failure).

## Adding a new test

1. Add an `AutomationId` to any element you need to address (and, for SkiaSharp
   controls, wrap in a `Border` host with `AutomationId` +
   `SemanticProperties.Description`).
2. Expose it on `MainPage` (`PageObjects/MainPage.cs`).
3. Write the test with `IClassFixture<AppFixture>` so each test class gets a
   freshly-launched app + driver session.
