namespace StellarAdmin.Resources;

// Turns an image field's stored value into the image's URL. Without a resolver the value is the URL
internal sealed class ImageUrlResolver
{
    private Func<string, IServiceProvider, CancellationToken, Task<string?>>? _resolve;

    public void Set(Func<string, string?> resolve)
    {
        ArgumentNullException.ThrowIfNull(resolve);

        _resolve = (value, _, _) => Task.FromResult(resolve(value));
    }

    public void Set(Func<string, IServiceProvider, CancellationToken, Task<string?>> resolve)
    {
        ArgumentNullException.ThrowIfNull(resolve);

        _resolve = resolve;
    }

    // The URL of a stored value, or null for an empty one
    public async Task<string?> ResolveAsync(
        object? value,
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return _resolve is null ? text : await _resolve(text, services, cancellationToken);
    }
}
