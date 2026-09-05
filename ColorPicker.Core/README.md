# ColorPicker.Maui.Core

Platform-independent color math and picker geometry used by
[ColorPicker.Maui](https://github.com/vpapenko/ColorPicker.Maui).

The package has no MAUI or SkiaSharp dependency and targets `netstandard2.0`
and `net8.0`.

## Install

```bash
dotnet add package ColorPicker.Maui.Core
```

`ColorPicker.Maui` already references this package, so MAUI applications do not
need to install it separately. Add it directly when color conversion, normalized
picker geometry, or interaction logic is needed in a platform-independent
project.

## Example

```csharp
using ColorPicker.Core;

var hsla = new HslaColor(h: 0.58, s: 0.75, l: 0.5);
RgbaColor rgba = hsla.ToRgba();

var disc = new HueSaturationDisc();
UnitPoint indicator = disc.ColorToPoint(hsla);

var updated = disc.UpdateColor(
    new UnitPoint(x: 0.8f, y: 0.25f),
    hsla);
```

## Included APIs

- `HslaColor`, `HsvaColor`, and `RgbaColor` value types and conversions
- normalized `UnitPoint` and `PolarPoint` geometry
- hue/saturation discs, hue and luminosity rings, and saturation/value triangles
- HSL, RGB, and alpha channel slider models
- stateful disc and triangle interaction controllers
- cycle-safe connection graphs
- DPI-aware indicator-radius calculations

All color channels and normalized coordinates use the range `0..1`.
