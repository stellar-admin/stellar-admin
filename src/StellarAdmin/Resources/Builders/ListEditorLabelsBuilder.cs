using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of list editors.
/// </summary>
public sealed class ListEditorLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the callback that generates the text of the button that adds an item.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add item</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> AddText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.AddText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the count shown on the add row when the list has a
    ///     maximum, with <c>{count}</c> for the number of items.
    /// </summary>
    /// <value>
    ///     The default returns <c>{count} of</c> followed by the maximum, such as <c>{count} of
    ///     8</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> CountText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.CountText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown when an item is already in the list,
    ///     with <c>{value}</c> for the item.
    /// </summary>
    /// <value>
    ///     The default returns <c>{value} is already in the list.</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> DuplicateText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.DuplicateText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the text shown when the list has no items.
    /// </summary>
    /// <value>
    ///     The default returns <c>No items yet</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> EmptyText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.EmptyText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the text shown when the list is full, and the error
    ///     when it has too many items.
    /// </summary>
    /// <value>
    ///     The default returns <c>You can add up to 8 items.</c> for a maximum of 8.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> FullText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.FullText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the screen reader label of an item, with <c>{n}</c> for
    ///     its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Item {n}</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> ItemLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.ItemLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown on the server when a text item is too
    ///     long.
    /// </summary>
    /// <value>
    ///     The default returns <c>Use 120 characters or fewer.</c> for a maximum length of 120.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> MaxLengthText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.MaxLengthText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the screen reader label of an item's drag handle, with
    ///     <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Move item {n}</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> MoveLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.MoveLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the text a read-only list shows when it has no items.
    /// </summary>
    /// <value>
    ///     The default returns <c>None</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> NoneText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.NoneText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown on the server when a text item doesn't
    ///     match the editor's pattern and the editor has no pattern message.
    /// </summary>
    /// <value>
    ///     The default returns <c>Match the requested format.</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> PatternText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.PatternText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown on the server when a number item is out
    ///     of range.
    /// </summary>
    /// <value>
    ///     The default returns <c>Enter a number from 1 to 31.</c> for a range from 1 to 31.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> RangeText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.RangeText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the screen reader label of an item's remove button,
    ///     with <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Remove item {n}</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> RemoveLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.RemoveLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown when the list has fewer items than its
    ///     minimum.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add at least 2 items.</c> for a minimum of 2.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ListEditorLabelContext, string> TooFewText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.ListEditor.TooFewText = value
            );
        }
    }

    internal ListEditorLabelsBuilder(IServiceCollection services) => _services = services;
}
