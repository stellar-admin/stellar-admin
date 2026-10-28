using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.ScenarioTesting.Runner;

// Starts the spec's scenario on a loopback port, runs its steps in a fresh browser context and stops
// both; a failing step throws SpecException with the spec's file and line
public static class SpecRunner
{
    // A null screenshot directory skips screenshot and record steps; configureLogging changes where the
    // scenario's host logs. Returns the number of steps run.
    public static async Task<int> RunAsync(
        Spec spec,
        SpecBrowser browser,
        string? screenshotDirectory = null,
        Action<ILoggingBuilder>? configureLogging = null
    )
    {
        var (app, baseUrl) = await ScenarioHost.StartLoopbackAsync(spec.Scenario, configureLogging);
        await using var _ = app;

        // Only a spec with record steps records, and only when screenshots are saved
        var record = spec.Steps.FirstOrDefault(step => step.Kind == "record");
        var videoDirectory =
            screenshotDirectory is not null && record is not null
                ? Path.Combine(screenshotDirectory, $".video-{Guid.NewGuid():N}")
                : null;

        var context = await browser.NewContextAsync(baseUrl, videoDirectory);
        try
        {
            var run = await StartAsync(spec, context, screenshotDirectory, record);
            var completed = await RunStepsAsync(spec, run);

            if (run.Video is { } video)
            {
                if (video.Current is not null)
                {
                    await video.EndAsync(run.Page);
                }

                // The video is only complete once the context closes
                var path = await run.Page.Video!.PathAsync();
                await context.CloseAsync();
                await video.SaveAsync(spec.Path, path, screenshotDirectory!);
            }

            return completed;
        }
        finally
        {
            await context.DisposeAsync();
            if (videoDirectory is not null && Directory.Exists(videoDirectory))
            {
                Directory.Delete(videoDirectory, recursive: true);
            }
        }
    }

    private static async Task<int> RunStepsAsync(Spec spec, SpecRun run)
    {
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

    // Video recording fails on the first page when Playwright's own ffmpeg build is missing
    private static async Task<SpecRun> StartAsync(
        Spec spec,
        IBrowserContext context,
        string? screenshotDirectory,
        SpecStep? record
    )
    {
        if (screenshotDirectory is null || record is null)
        {
            return new(await context.NewPageAsync(), screenshotDirectory);
        }

        await SpecVideo.InstallAsync(context);
        try
        {
            var page = await context.NewPageAsync();

            return new(page, screenshotDirectory, new SpecVideo());
        }
        catch (PlaywrightException exception) when (exception.Message.Contains("ffmpeg"))
        {
            throw new SpecException(
                spec.Path,
                record.Line,
                "Recording needs Playwright's ffmpeg build; install it with: pwsh bin/Debug/net10.0/playwright.ps1 install ffmpeg",
                exception
            );
        }
    }
}
