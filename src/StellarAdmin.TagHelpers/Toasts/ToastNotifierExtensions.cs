namespace StellarAdmin.TagHelpers;

/// <summary>
///     Shortcuts for adding toasts of each type.
/// </summary>
public static class ToastNotifierExtensions
{
    extension(IToastNotifier notifier)
    {
        /// <summary>
        ///     Adds an error toast.
        /// </summary>
        /// <param name="title">The toast title.</param>
        /// <param name="description">Optional text shown below the title.</param>
        public void Error(string title, string? description = null) =>
            notifier.Add(ToastType.Error, title, description);

        /// <summary>
        ///     Adds an informational toast.
        /// </summary>
        /// <param name="title">The toast title.</param>
        /// <param name="description">Optional text shown below the title.</param>
        public void Info(string title, string? description = null) =>
            notifier.Add(ToastType.Info, title, description);

        /// <summary>
        ///     Adds a success toast.
        /// </summary>
        /// <param name="title">The toast title.</param>
        /// <param name="description">Optional text shown below the title.</param>
        public void Success(string title, string? description = null) =>
            notifier.Add(ToastType.Success, title, description);

        /// <summary>
        ///     Adds a warning toast.
        /// </summary>
        /// <param name="title">The toast title.</param>
        /// <param name="description">Optional text shown below the title.</param>
        public void Warning(string title, string? description = null) =>
            notifier.Add(ToastType.Warning, title, description);

        private void Add(ToastType type, string title, string? description) =>
            notifier.Add(
                new Toast
                {
                    Title = title,
                    Description = description,
                    Type = type,
                }
            );
    }
}
