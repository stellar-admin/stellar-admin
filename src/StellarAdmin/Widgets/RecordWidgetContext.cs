using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Widgets;

/// <summary>
///     Describes a resource's view or edit page that a widget renders on.
/// </summary>
/// <typeparam name="TModel">The type of the model the page shows or edits.</typeparam>
public sealed class RecordWidgetContext<TModel> : WidgetContext
{
    /// <summary>
    ///     Gets the key of the record the page shows or edits.
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Gets the model the page shows or edits.
    /// </summary>
    /// <value>
    ///     The loaded model. When an edit form is shown again because its values aren't valid,
    ///     the model has the submitted values.
    /// </value>
    public TModel Record { get; }

    internal RecordWidgetContext(HttpContext httpContext, string key, TModel record)
        : base(httpContext)
    {
        Key = key;
        Record = record;
    }
}
