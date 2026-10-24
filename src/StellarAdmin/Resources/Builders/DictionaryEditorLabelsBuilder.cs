using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of dictionary editors.
/// </summary>
public sealed class DictionaryEditorLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the callback that generates the text of the button that adds an entry.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add item</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> AddText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.AddText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the count shown on the add row when the dictionary has
    ///     a maximum, with <c>{count}</c> for the number of entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>{count} of</c> followed by the maximum, such as <c>{count} of
    ///     8</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> CountText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.CountText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown when a key is already used, with
    ///     <c>{value}</c> for the key.
    /// </summary>
    /// <value>
    ///     The default returns <c>{value} is already used.</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> DuplicateText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.DuplicateText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the text shown when the dictionary has no entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>No items yet</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> EmptyText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.EmptyText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the text shown when the dictionary is full, and the
    ///     error when it has too many entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>You can add up to 8 items.</c> for a maximum of 8.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> FullText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.FullText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the label of the key column.
    /// </summary>
    /// <value>
    ///     The default returns <c>Key</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> KeyLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.KeyLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown on the server when a text key or value
    ///     is too long.
    /// </summary>
    /// <value>
    ///     The default returns <c>Use 120 characters or fewer.</c> for a maximum length of 120.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> MaxLengthText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.MaxLengthText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the text a read-only dictionary shows when it has no
    ///     entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>None</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> NoneText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.NoneText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown on the server when a text key or value
    ///     doesn't match its pattern and has no pattern message.
    /// </summary>
    /// <value>
    ///     The default returns <c>Match the requested format.</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> PatternText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.PatternText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown on the server when a number value is
    ///     out of range.
    /// </summary>
    /// <value>
    ///     The default returns <c>Enter a number from 0 to 50.</c> for a range from 0 to 50.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> RangeText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.RangeText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the screen reader label of an entry's remove button,
    ///     with <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Remove row {n}</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> RemoveLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.RemoveLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown when an entry has a key and no value,
    ///     or a value and no key.
    /// </summary>
    /// <value>
    ///     The default returns the column label followed by <c>is required.</c>, such as
    ///     <c>Language is required.</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> RequiredText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.RequiredText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the error shown when the dictionary has fewer entries
    ///     than its minimum.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add at least 2 items.</c> for a minimum of 2.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> TooFewText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.TooFewText = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the label of the value column.
    /// </summary>
    /// <value>
    ///     The default returns <c>Value</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<DictionaryEditorLabelContext, string> ValueLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DictionaryEditor.ValueLabel = value
            );
        }
    }

    internal DictionaryEditorLabelsBuilder(IServiceCollection services) => _services = services;
}
