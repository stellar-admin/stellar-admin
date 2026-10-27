using Microsoft.Playwright;

namespace StellarAdmin.Scenarios.Specs;

// What a step runs against; a null screenshot directory skips screenshot steps
public sealed record SpecRun(IPage Page, string? ScreenshotDirectory);
