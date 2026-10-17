namespace StellarAdmin.UI.Toasts;

/// <summary>
///     Queues toasts to show on the next page or in the response to the current script request.
/// </summary>
public interface IToastNotifier
{
    /// <summary>
    ///     Adds a toast to the queue.
    /// </summary>
    /// <param name="toast">The toast to show.</param>
    void Add(Toast toast);
}
