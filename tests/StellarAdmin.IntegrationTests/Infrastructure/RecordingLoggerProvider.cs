using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.IntegrationTests.Infrastructure;

// Records what the app logs, so tests can check the configuration problems the admin panel logged. The test host
// registers one per app.
internal sealed class RecordingLoggerProvider : ILoggerProvider
{
    public const string ConfigurationCategory = "StellarAdmin.Configuration";

    private readonly ConcurrentQueue<RecordedLog> _logs = new();

    public IReadOnlyList<RecordedLog> Logs => [.. _logs];

    // The configuration problems, at every level, in the order they were logged
    public IReadOnlyList<RecordedLog> ConfigurationProblems =>
        [
            .. _logs.Where(log =>
                log.Category == ConfigurationCategory && log.Level >= LogLevel.Information
            ),
        ];

    public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, _logs);

    public void Dispose() { }

    private sealed class RecordingLogger(string category, ConcurrentQueue<RecordedLog> logs)
        : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) => logs.Enqueue(new(category, logLevel, eventId, formatter(state, exception)));
    }
}

internal sealed record RecordedLog(
    string Category,
    LogLevel Level,
    EventId EventId,
    string Message
);
