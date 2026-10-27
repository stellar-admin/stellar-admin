using Microsoft.Playwright;

namespace StellarAdmin.Scenarios.Specs;

// What a step runs against; a null screenshot directory skips screenshot and recording steps. While a
// recording runs, Page is the recorded page
public sealed class SpecRun(
    SpecBrowser browser,
    string baseUrl,
    IPage page,
    string? screenshotDirectory
)
{
    public SpecBrowser Browser { get; } = browser;

    public string BaseUrl { get; } = baseUrl;

    public IPage Page { get; set; } = page;

    public SpecRecording? Recording { get; set; }

    public string? ScreenshotDirectory { get; } = screenshotDirectory;
}
