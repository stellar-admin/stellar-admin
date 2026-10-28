using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Playwright;

namespace StellarAdmin.ScenarioTesting.Runner;

// Playwright can only record a whole context, so a spec with record steps records its whole run and each
// record to recordEnd stretch is cut out afterwards as {name}.mp4, plus {name}.gif when asked for. While a
// clip runs, motion is on and a drawn cursor glides to each click
public sealed class SpecVideo
{
    private static readonly string CursorScript = ReadCursorScript();

    private readonly List<(SpecRecordOptions Options, TimeSpan Start, TimeSpan End)> _clips = [];

    // Started with the page, which is when the video starts
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private TimeSpan _start;

    public SpecRecordOptions? Current { get; private set; }

    public static Task InstallAsync(IBrowserContext context) =>
        context.AddInitScriptAsync(CursorScript);

    public async Task BeginAsync(IPage page, SpecRecordOptions options)
    {
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.NoPreference });
        await page.EvaluateAsync("() => window.specCursor.show()");

        // The cursor starts in the middle of the page until the mouse has moved
        if (await CursorAsync(page) is null && page.ViewportSize is { } viewport)
        {
            await page.Mouse.MoveAsync(viewport.Width / 2f, viewport.Height / 2f);
        }

        Current = options;
        _start = _clock.Elapsed;
    }

    // Holds the last frame for the clip's hold time before the clip ends
    public async Task EndAsync(IPage page)
    {
        var options = Current!;
        await page.WaitForTimeoutAsync(options.Hold);
        _clips.Add((options, _start, _clock.Elapsed));
        Current = null;

        await page.EvaluateAsync("() => window.specCursor.hide()");
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
    }

    // Scrolls the element into view, eases the cursor to its centre and rests briefly before the step acts
    public async Task MoveToAsync(ILocator locator, float timeout)
    {
        await ScrollToAsync(locator, timeout);
        var box =
            await locator.BoundingBoxAsync(new() { Timeout = timeout })
            ?? throw new InvalidOperationException("The element has no size.");
        await MoveAsync(locator.Page, box.X + box.Width / 2, box.Y + box.Height / 2);
        await locator.Page.WaitForTimeoutAsync(150);
    }

    // Cuts the clips from the finished video with ffmpeg
    public async Task SaveAsync(string specPath, string video, string directory)
    {
        foreach (var (options, start, end) in _clips)
        {
            var range = $"-ss {Seconds(start)} -to {Seconds(end)} -i \"{video}\"";
            var output = Path.Combine(directory, options.Name);
            try
            {
                await FfmpegAsync(
                    $"{range} -c:v libx264 -pix_fmt yuv420p -crf 18 -movflags +faststart \"{output}.mp4\""
                );
                if (options.Gif)
                {
                    await FfmpegAsync(
                        $"{range} -vf \"fps=20,split[a][b];[a]palettegen=stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle\" \"{output}.gif\""
                    );
                }
            }
            catch (Win32Exception exception)
            {
                throw new SpecException(
                    specPath,
                    options.Line,
                    "Saving a recording needs ffmpeg on the PATH.",
                    exception
                );
            }
        }
    }

    // Smooth-scrolls the element to the middle of the view unless it is already in view; resolves when the
    // scroll ends
    public static async Task ScrollToAsync(ILocator locator, float timeout)
    {
        await locator.WaitForAsync(new() { Timeout = timeout });
        await locator.EvaluateAsync(
            """
            element => new Promise(resolve => {
              const rect = element.getBoundingClientRect();
              if (rect.top >= 0 && rect.bottom <= innerHeight && rect.left >= 0 && rect.right <= innerWidth) {
                return resolve();
              }
              addEventListener("scrollend", resolve, { capture: true, once: true });
              setTimeout(resolve, 2000);
              element.scrollIntoView({ behavior: "smooth", block: "center" });
            })
            """
        );
    }

    private static Task<float[]?> CursorAsync(IPage page) =>
        page.EvaluateAsync<float[]?>("() => window.specCursor.position()");

    private static async Task FfmpegAsync(string arguments)
    {
        using var process =
            Process.Start(
                new ProcessStartInfo("ffmpeg", $"-y -loglevel error {arguments}")
                {
                    RedirectStandardError = true,
                }
            ) ?? throw new InvalidOperationException("ffmpeg did not start.");
        var errors = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"ffmpeg failed: {errors}");
        }
    }

    private static string ReadCursorScript()
    {
        using var stream = typeof(SpecVideo).Assembly.GetManifestResourceStream("cursor.js")!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private static string Seconds(TimeSpan time) =>
        time.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);

    // About 60 moves a second along an ease-in-out curve; longer distances take a little longer
    private async Task MoveAsync(IPage page, float x, float y)
    {
        var from = await CursorAsync(page) ?? [x, y];
        var distance = MathF.Sqrt((x - from[0]) * (x - from[0]) + (y - from[1]) * (y - from[1]));
        var duration = Math.Clamp(250 + distance * 0.6f, 300, 900) / Current!.CursorSpeed;
        var steps = Math.Max(1, (int)(duration / 16));
        for (var i = 1; i <= steps; i++)
        {
            var t = (float)i / steps;
            var eased = t < 0.5f ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;
            await page.Mouse.MoveAsync(
                from[0] + (x - from[0]) * eased,
                from[1] + (y - from[1]) * eased
            );
            await page.WaitForTimeoutAsync(16);
        }
    }
}
