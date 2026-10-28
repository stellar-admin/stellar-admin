using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StellarAdmin.Cli;

// Holds a host's log messages until the caller drains them
internal sealed class CollectingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<(LogLevel Level, string Message)> messages = new();

    public ILogger CreateLogger(string categoryName) => new Logger(messages);

    // The messages logged since the last call
    public List<(LogLevel Level, string Message)> Drain()
    {
        List<(LogLevel Level, string Message)> drained = [];
        while (messages.TryDequeue(out var message))
            drained.Add(message);

        return drained;
    }

    public void Dispose() { }

    private sealed class Logger(ConcurrentQueue<(LogLevel Level, string Message)> messages)
        : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) =>
            messages.Enqueue(
                (
                    logLevel,
                    exception is null
                        ? formatter(state, exception)
                        : $"{formatter(state, exception)}\n{exception}"
                )
            );
    }
}
