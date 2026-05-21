// Disable xUnit's default parallel-collection execution.
// All our test fixtures launch the same ColorPickerTestApp.exe and drive
// a single Appium server; running collections in parallel causes
// WebDriverException "Request failed with status code 500" and racy
// tap-coordinates issues.
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
