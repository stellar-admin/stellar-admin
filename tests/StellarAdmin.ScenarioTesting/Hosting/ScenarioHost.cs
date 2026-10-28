using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.ScenarioTesting.Hosting;

public static class ScenarioHost
{
    // Hosts the scenario on urls, or Kestrel's default address, until the host shuts down, such as on
    // Ctrl+C; started gets the addresses it listens on
    public static async Task RunAsync(
        Scenario scenario,
        string? urls,
        Action<IReadOnlyCollection<string>> started
    )
    {
        await using var factory = new ScenarioApplicationFactory(
            scenario,
            builder =>
            {
                if (urls is not null)
                    builder.UseUrls(urls);
            }
        );
        factory.UseKestrel();
        factory.StartServer();
        started(factory.Addresses);

        var stopping = new TaskCompletionSource();
        using var _ = factory
            .Services.GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStopping.Register(() => stopping.TrySetResult());
        await stopping.Task;
    }

    // Real server on a free loopback port for the browser; logs only warnings and errors, to the
    // providers configureLogging leaves
    public static (IAsyncDisposable Host, string BaseUrl) StartLoopback(
        Scenario scenario,
        Action<ILoggingBuilder>? configureLogging = null
    )
    {
        var factory = new ScenarioApplicationFactory(
            scenario,
            builder =>
                builder.ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Warning);
                    configureLogging?.Invoke(logging);
                })
        );
        factory.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        factory.StartServer();

        return (factory, factory.Addresses.First());
    }
}
