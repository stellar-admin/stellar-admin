using Microsoft.Playwright;

namespace StellarAdmin.ScenarioTesting.Runner;

// What a step runs against; a null screenshot directory skips screenshot and record steps. Requests holds
// every request the context's pages make. Video is set when screenshots are saved and the spec has record
// steps
public sealed record SpecRun(
    IPage Page,
    SpecRequests Requests,
    string? ScreenshotDirectory,
    SpecVideo? Video = null
)
{
    // Inside a within step, the element locators search in; a locator without selector or role is this
    // element
    public ILocator? Scope { get; init; }

    // The clip being recorded, if any
    public SpecRecordOptions? Recording => Video?.Current;
}
