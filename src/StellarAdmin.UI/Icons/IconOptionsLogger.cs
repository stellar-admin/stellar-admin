using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StellarAdmin.UI.Icons;

// Builds the icon options when the app starts, so mistakes in the icon registrations are logged once, as warnings in
// the same category as the admin panel's configuration problems
internal sealed class IconOptionsLogger(IOptions<IconOptions> options) : IHostedService
{
    private const string Category = "StellarAdmin.Configuration";

    // The event ID follows the admin panel's configuration problem IDs
    private static readonly EventId SkippedIconMapping = new(1090, nameof(SkippedIconMapping));

    public static void Log(IconOptions options, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(Category);
        foreach (var problem in options.Problems)
        {
            logger.LogWarning(
                SkippedIconMapping,
                "Configuration problem in the icons: {Problem}",
                problem
            );
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = options.Value;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
