# ColorPicker.Maui

ColorPicker controls for .NET MAUI

## Features

This library provides customizable color picker controls for .NET MAUI applications:
- **ColorWheel**: Interactive color wheel with optional luminosity and alpha sliders
- **ColorTriangle**: Triangle color picker that can be attached to the color wheel
- **RGB Sliders**: Individual sliders for Red, Green, Blue, and Alpha channels
- **HSL Sliders**: Individual sliders for Hue, Saturation, Lightness, and Alpha channels

## Prerequisites

- .NET 8 SDK or later
- .NET MAUI workload installed

### Installing Prerequisites

1. Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

2. Install MAUI workload:
   ```bash
   dotnet workload install maui-android  # For Android development
   dotnet workload install maui-ios      # For iOS development (macOS only)
   dotnet workload install maui-maccatalyst  # For Mac Catalyst (macOS only)
   dotnet workload install maui-windows  # For Windows development (Windows only)
   ```

## Building the Project

### Build the library

```bash
dotnet build ColorPicker/ColorPicker.csproj --configuration Release
```

### Build the entire solution (library + sample app)

```bash
dotnet build ColorPicker.Maui.sln --configuration Release
```

## Running the Sample App

The `ColorPickerTestApp` project is a sample MAUI application that demonstrates all the color picker controls.

### Run on Android

```bash
cd ColorPickerTestApp
dotnet build -f net8.0-android
# Deploy to connected Android device or emulator
dotnet run -f net8.0-android
```

### Run on Windows (Windows only)

```bash
cd ColorPickerTestApp
dotnet build -f net8.0-windows10.0.19041.0
dotnet run -f net8.0-windows10.0.19041.0
```

### Run on iOS/Mac Catalyst (macOS only)

```bash
cd ColorPickerTestApp
dotnet build -f net8.0-ios
# or
dotnet build -f net8.0-maccatalyst
```

## Using in Your Project

### 1. Add the library reference

Add a project reference to the ColorPicker library in your .csproj file:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/ColorPicker/ColorPicker.csproj" />
</ItemGroup>
```

### 2. Register the controls in MauiProgram.cs

```csharp
using ColorPicker.Classes;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        
        // Register ColorPicker controls and SkiaSharp
        builder.UseColorPickersAndSliders();
        
        return builder.Build();
    }
}
```

### 3. Use in XAML

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:controls="clr-namespace:ColorPicker.Controls;assembly=ColorPicker"
             x:Class="YourApp.MainPage">
    
    <controls:ColorWheel x:Name="ColorWheel1"
                         SelectedColor="{Binding SelectedColor}"
                         ShowAlphaSlider="True"
                         ShowLuminositySlider="True" />
                         
</ContentPage>
```

## Sample Usage

See the `ColorPickerTestApp` project for comprehensive examples of all controls and their configurations.

## Dependencies

- Microsoft.Maui.Controls 8.0.100+
- SkiaSharp 2.88.8+
- SkiaSharp.Views.Maui.Controls 2.88.8+
- ColorMinePortable 2.0.1+

## License

See LICENSE file for details.
