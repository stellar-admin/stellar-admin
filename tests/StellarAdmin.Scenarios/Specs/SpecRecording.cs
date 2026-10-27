using System.Diagnostics;
using Microsoft.Playwright;

namespace StellarAdmin.Scenarios.Specs;

// Records the steps between record and recordEnd as a video. Playwright records a whole context, so the
// recording opens the current page in a new context with a drawn cursor, and the spec carries on in the
// original page, at the recording's final URL, once it ends
public sealed class SpecRecording
{
    private static readonly string CursorScript = ReadCursorScript();

    private readonly IBrowserContext _context;
    private readonly string _directory;

    // The blank and loading frames before the page was ready, trimmed from the start of the video
    private readonly TimeSpan _lead;
    private readonly string _name;
    private readonly IPage _original;
    private float _x;
    private float _y;

    private SpecRecording(
        string name,
        string directory,
        IBrowserContext context,
        IPage original,
        TimeSpan lead,
        float x,
        float y
    )
    {
        _name = name;
        _directory = directory;
        _context = context;
        _original = original;
        _lead = lead;
        _x = x;
        _y = y;
    }

    public static async Task StartAsync(SpecRun run, string name)
    {
        var original = run.Page;
        var viewport = original.ViewportSize ?? new() { Width = 1280, Height = 800 };
        var scheme = await original.EvaluateAsync<string>("document.documentElement.className");
        var directory = Path.Combine(run.ScreenshotDirectory!, ".recording-" + name);
        var context = await run.Browser.NewRecordingContextAsync(run.BaseUrl, viewport, directory);
        await context.AddInitScriptAsync(CursorScript);

        var started = Stopwatch.StartNew();
        var page = await context.NewPageAsync();
        await page.GotoAsync(original.Url);
        await page.EvaluateAsync("scheme => document.documentElement.className = scheme", scheme);
        // The cursor starts in the middle of the page
        float x = viewport.Width / 2f;
        float y = viewport.Height / 2f;
        await page.Mouse.MoveAsync(x, y);
        await page.WaitForTimeoutAsync(200);

        run.Page = page;
        run.Recording = new(name, directory, context, original, started.Elapsed, x, y);
    }

    // Eases the cursor to the element's centre, rests briefly and clicks
    public async Task ClickAsync(ILocator locator, float timeout)
    {
        await locator.WaitForAsync(new() { Timeout = timeout });
        await locator.ScrollIntoViewIfNeededAsync(new() { Timeout = timeout });
        var box =
            await locator.BoundingBoxAsync(new() { Timeout = timeout })
            ?? throw new InvalidOperationException("The element has no size.");
        await MoveAsync(locator.Page, box.X + box.Width / 2, box.Y + box.Height / 2);
        await locator.Page.WaitForTimeoutAsync(150);
        await locator.ClickAsync(new() { Timeout = timeout });
    }

    // Holds the last frame, closes the context to finish the video and converts it to MP4 and GIF
    public async Task FinishAsync(SpecRun run)
    {
        var page = run.Page;
        await page.WaitForTimeoutAsync(800);
        var url = page.Url;
        var video = page.Video!;
        await _context.CloseAsync();
        var webm = Path.Combine(run.ScreenshotDirectory!, $"{_name}.webm");
        await video.SaveAsAsync(webm);
        await video.DeleteAsync();
        Directory.Delete(_directory, recursive: true);

        run.Page = _original;
        run.Recording = null;
        if (_original.Url != url)
        {
            await _original.GotoAsync(url);
        }

        await ConvertAsync(webm);
    }

    private static string ReadCursorScript()
    {
        using var stream = typeof(SpecRecording).Assembly.GetManifestResourceStream("cursor.js")!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    // Keeps the WebM when ffmpeg is missing
    private async Task ConvertAsync(string webm)
    {
        var start = _lead.TotalSeconds.ToString(
            "0.###",
            System.Globalization.CultureInfo.InvariantCulture
        );
        var mp4 = Path.ChangeExtension(webm, ".mp4");
        var gif = Path.ChangeExtension(webm, ".gif");
        try
        {
            await FfmpegAsync(
                $"-y -loglevel error -ss {start} -i \"{webm}\" -c:v libx264 -pix_fmt yuv420p -crf 18 -movflags +faststart \"{mp4}\""
            );
            await FfmpegAsync(
                $"-y -loglevel error -ss {start} -i \"{webm}\" -vf \"fps=20,split[a][b];[a]palettegen=stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle\" \"{gif}\""
            );
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return;
        }

        File.Delete(webm);
    }

    private static async Task FfmpegAsync(string arguments)
    {
        using var process =
            Process.Start(
                new ProcessStartInfo("ffmpeg", arguments) { RedirectStandardError = true }
            ) ?? throw new InvalidOperationException("ffmpeg did not start.");
        var errors = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"ffmpeg failed: {errors}");
        }
    }

    // About 60 moves a second along an ease-in-out curve; longer distances take a little longer
    private async Task MoveAsync(IPage page, float x, float y)
    {
        var distance = MathF.Sqrt((x - _x) * (x - _x) + (y - _y) * (y - _y));
        var duration = Math.Clamp(250 + distance * 0.6f, 300, 900);
        var steps = (int)(duration / 16);
        for (var i = 1; i <= steps; i++)
        {
            var t = (float)i / steps;
            var eased = t < 0.5f ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;
            await page.Mouse.MoveAsync(_x + (x - _x) * eased, _y + (y - _y) * eased);
            await page.WaitForTimeoutAsync(16);
        }

        _x = x;
        _y = y;
    }
}
