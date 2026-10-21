using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.UI.Tests.Support;

// Records what the services log, so tests can check the configuration problems that were logged
internal sealed class RecordingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<RecordedLog> _logs = new();

    public IReadOnlyList<RecordedLog> Logs => [.. _logs];

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
