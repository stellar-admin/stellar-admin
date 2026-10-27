using Microsoft.Playwright;

namespace StellarAdmin.Scenarios.Tests.Specs;

// What a step runs against; a null screenshot directory skips screenshot steps
internal sealed record SpecRun(IPage Page, string? ScreenshotDirectory);
