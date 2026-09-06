# CoreConsumerSmoke

Compile-only project that consumes the packed `ColorPicker.Maui.Core` NuGet
package for both `netstandard2.0` and `net8.0`. CI uses it to catch missing
assets, dependency errors, and accidental public API removals.
