namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an alert dialog.
/// </summary>
public enum AlertDialogSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size.
    /// </summary>
    Small,
}

internal static class AlertDialogSizeExtensions
{
    extension(AlertDialogSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                AlertDialogSize.Default => "default",
                AlertDialogSize.Small => "sm",
                _ => string.Empty,
            };
    }
}
