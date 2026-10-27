namespace StellarAdmin.Scenarios.Specs;

// Starts the spec's scenario on a loopback port, runs its steps in a fresh browser context and stops
// both; a failing step throws SpecException with the spec's file and line
public static class SpecRunner
{
    // A null screenshot directory skips screenshot steps; returns the number of steps run
    public static async Task<int> RunAsync(
        Spec spec,
        SpecBrowser browser,
        string? screenshotDirectory = null
    )
    {
        var (app, baseUrl) = await ScenarioHost.StartLoopbackAsync(spec.Scenario);
        await using var _ = app;
        await using var context = await browser.NewContextAsync(baseUrl);
        var run = new SpecRun(await context.NewPageAsync(), screenshotDirectory);

        var completed = 0;
        foreach (var step in spec.Steps)
        {
            try
            {
                await step.RunAsync(run);
            }
            catch (Exception exception) when (exception is not SpecException)
            {
                throw new SpecException(
                    spec.Path,
                    step.Line,
                    $"{step.Kind}: {exception.Message}",
                    exception
                );
            }

            completed++;
        }

        return completed;
    }
}
