using ColorPicker.Controls;
using ColorPicker.Core;

var mauiAssembly = typeof(ColorWheel).Assembly;
var coreAssembly = typeof(HslaColor).Assembly;

const string expectedCoreAssemblyName = "ColorPicker.Maui.Core";
if (coreAssembly.GetName().Name != expectedCoreAssemblyName)
{
    throw new InvalidOperationException(
        $"Expected Core assembly '{expectedCoreAssemblyName}', got '{coreAssembly.GetName().Name}'.");
}

var publicCoreTypes = coreAssembly.ExportedTypes
    .Select(type => type.FullName!)
    .OrderBy(name => name, StringComparer.Ordinal)
    .ToArray();
var forwardedCoreTypes = mauiAssembly.GetForwardedTypes()
    .Select(type => type.FullName!)
    .OrderBy(name => name, StringComparer.Ordinal)
    .ToArray();

var missingForwarders = publicCoreTypes.Except(forwardedCoreTypes, StringComparer.Ordinal).ToArray();
var unexpectedForwarders = forwardedCoreTypes.Except(publicCoreTypes, StringComparer.Ordinal).ToArray();
if (missingForwarders.Length != 0 || unexpectedForwarders.Length != 0)
{
    throw new InvalidOperationException(
        $"Core type forwarders differ. Missing: [{string.Join(", ", missingForwarders)}]. " +
        $"Unexpected: [{string.Join(", ", unexpectedForwarders)}].");
}

var legacyType = Type.GetType($"{typeof(HslaColor).FullName}, ColorPicker", throwOnError: true)!;
if (legacyType.Assembly != coreAssembly)
{
    throw new InvalidOperationException(
        $"Legacy ColorPicker type lookup resolved to '{legacyType.Assembly.GetName().Name}'.");
}

Console.WriteLine(
    $"Verified {forwardedCoreTypes.Length} Core type forwarders to {expectedCoreAssemblyName}.");
