namespace StellarAdmin.Widgets;

/// <summary>
///     Marks a property of a widget's parameters that the widget's own request can change, such
///     as a period picker in the widget.
/// </summary>
/// <remarks>
///     The other properties keep the values that the widget's <c>Parameters</c> delegate
///     returned. A value from the request is bound with MVC model binding, from the query string
///     under the property's name. On a positional record, apply the attribute to the property:
///     <c>[property: WidgetInput]</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class WidgetInputAttribute : Attribute;
