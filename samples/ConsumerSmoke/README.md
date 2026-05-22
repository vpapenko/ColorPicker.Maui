# ConsumerSmoke

Tiny MAUI class library that consumes **ColorPicker.Maui** as a
`PackageReference` (not a `ProjectReference`). Its sole purpose is to be
built by CI against the freshly-packed `.nupkg` so packaging regressions
(missing `.targets`, broken MAUI resource glob, dropped public type, TFM
mismatch, transitive-dep hole) fail loudly before consumers ever see them.

## How it's used

| Trigger          | Source of the package          | Workflow job                       |
|------------------|--------------------------------|------------------------------------|
| Every PR         | Local feed = packed `nupkgs/`  | `build-and-test.yml → consumer-smoke` |
| Push to `main`   | GitHub Packages (just-published) | `ci.yml → consumer-e2e-github-packages` |

The version restored is supplied by CI via:

```bash
dotnet restore samples/ConsumerSmoke/ConsumerSmoke.csproj \
  -p:ColorPickerVersion=<the-version-just-packed-or-published>
```

## Run locally

```powershell
# After running `dotnet pack ColorPicker/ColorPicker.csproj -o nupkgs`:
dotnet nuget add source (Resolve-Path ./nupkgs) -n local-colorpicker --configfile samples/ConsumerSmoke/NuGet.config
dotnet restore samples/ConsumerSmoke/ConsumerSmoke.csproj -p:ColorPickerVersion=<version>
dotnet build samples/ConsumerSmoke/ConsumerSmoke.csproj -f net8.0-windows10.0.19041.0
```
