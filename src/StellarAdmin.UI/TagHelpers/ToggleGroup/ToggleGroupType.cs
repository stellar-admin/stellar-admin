namespace StellarAdmin.UI;

/// <summary>
///     Specifies how many items of a toggle group can be selected.
/// </summary>
public enum ToggleGroupType
{
    /// <summary>
    ///     One item can be selected at a time, and the group binds a single value.
    /// </summary>
    Single,

    /// <summary>
    ///     Any number of items can be selected, and the group binds a collection.
    /// </summary>
    Multiple,
}

internal static class ToggleGroupTypeExtensions
{
    extension(ToggleGroupType type)
    {
        public string GetDataAttributeText() =>
            type switch
            {
                ToggleGroupType.Single => "single",
                ToggleGroupType.Multiple => "multiple",
                _ => string.Empty,
            };
    }
}
