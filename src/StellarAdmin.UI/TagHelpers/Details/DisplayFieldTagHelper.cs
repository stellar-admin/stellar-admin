using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a read-only value with a label.
/// </summary>
/// <remarks>
///     The value is the element's content, or the value of the <c>asp-for</c> property. A
///     property value renders through a display template when the field sets <c>template</c> or
///     the property has a <c>[UIHint]</c>.
/// </remarks>
[HtmlTargetElement("sa-display-field")]
public sealed class DisplayFieldTagHelper : StellarAdminTagHelperBase, IGridPlacement
{
    private const string DefaultEmptyText = "—";

    private readonly IOptions<IconOptions> _iconOptions;
    private readonly ICompositeViewEngine _viewEngine;

    /// <summary>
    ///     Gets or sets the number of columns the field spans in an <c>sa-grid</c> or
    ///     <c>sa-details</c>.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c> for every column. The default is <c>1</c>.
    /// </value>
    /// <remarks>
    ///     Limited to the grid's number of columns at each width.
    /// </remarks>
    [HtmlAttributeName("column-span")]
    public string? ColumnSpan { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the field spans when the grid is at least 56rem
    ///     wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c>. The default is the span at 40rem.
    /// </value>
    [HtmlAttributeName("column-span-lg")]
    public string? ColumnSpanLarge { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the field spans when the grid is at least 40rem
    ///     wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c>. The default is the span at 30rem.
    /// </value>
    [HtmlAttributeName("column-span-md")]
    public string? ColumnSpanMedium { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns the field spans when the grid is at least 30rem
    ///     wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>full</c>. The default is the span at the smallest width.
    /// </value>
    [HtmlAttributeName("column-span-sm")]
    public string? ColumnSpanSmall { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at in an <c>sa-grid</c> or <c>sa-details</c>.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c> to follow the previous item. The default is
    ///     <c>auto</c>.
    /// </value>
    /// <remarks>
    ///     Ignored at a width where the grid has one column or the field would extend past the
    ///     last column.
    /// </remarks>
    [HtmlAttributeName("column-start")]
    public string? ColumnStart { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at when the grid is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c>. The default is the start at 40rem.
    /// </value>
    [HtmlAttributeName("column-start-lg")]
    public string? ColumnStartLarge { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at when the grid is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c>. The default is the start at 30rem.
    /// </value>
    [HtmlAttributeName("column-start-md")]
    public string? ColumnStartMedium { get; set; }

    /// <summary>
    ///     Gets or sets the column the field starts at when the grid is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12, or <c>auto</c>. The default is the start at the smallest width.
    /// </value>
    [HtmlAttributeName("column-start-sm")]
    public string? ColumnStartSmall { get; set; }

    /// <summary>
    ///     Gets or sets the text shown when the value is empty.
    /// </summary>
    /// <value>
    ///     The text for a null or blank value, or an empty collection. The default is the
    ///     property's null display text, or a dash when it has none.
    /// </value>
    /// <remarks>
    ///     Not used when <c>template-renders-empty</c> passes empty values to the template.
    /// </remarks>
    [HtmlAttributeName("empty-text")]
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets the model property whose label and value are shown.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets or sets the composite format string applied to the value.
    /// </summary>
    /// <value>
    ///     A format string, such as <c>{0:C}</c>. The default is the property's display format.
    /// </value>
    /// <remarks>
    ///     Applies to each item of a collection. Ignored when the value renders through a display
    ///     template.
    /// </remarks>
    [HtmlAttributeName("format")]
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets the URL the value links to.
    /// </summary>
    /// <remarks>
    ///     Applies only to a value from <c>asp-for</c> that renders as text. Without it, an email
    ///     address or web URL property links to itself.
    /// </remarks>
    [HtmlAttributeName("href")]
    public string? Href { get; set; }

    /// <summary>
    ///     Gets or sets the field label.
    /// </summary>
    /// <value>
    ///     The label text. The default is the property's display name.
    /// </value>
    /// <remarks>
    ///     Required when the field has no <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    ///     Gets or sets how the field arranges its label and value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the arrangement. The default is the
    ///     orientation of the enclosing details list, or vertical outside one.
    /// </value>
    [HtmlAttributeName("orientation")]
    public DisplayFieldOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets muted text shown after the value.
    /// </summary>
    /// <remarks>
    ///     Not shown when the value is empty.
    /// </remarks>
    [HtmlAttributeName("secondary")]
    public string? Secondary { get; set; }

    /// <summary>
    ///     Gets or sets the name of the display template that renders the value.
    /// </summary>
    /// <value>
    ///     A name that resolves like a partial view named <c>DisplayTemplates/{template}</c>. A
    ///     path that starts with <c>~/</c> or <c>/</c> is used as is. The default is the
    ///     property's UI hint.
    /// </value>
    /// <remarks>
    ///     Ignored without <c>asp-for</c>. The UI hint comes from <c>[UIHint]</c>.
    /// </remarks>
    [HtmlAttributeName("template")]
    public string? Template { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the display template also renders empty
    ///     values.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the template renders empty values instead of the empty text;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Has no effect without a template.
    /// </remarks>
    [HtmlAttributeName("template-renders-empty")]
    public bool? TemplateRendersEmpty { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DisplayFieldTagHelper" /> class.
    /// </summary>
    /// <param name="viewEngine">The view engine that finds display templates.</param>
    /// <param name="iconOptions">The icon options that supply the icons of Boolean values.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="viewEngine" /> or <paramref name="iconOptions" /> is
    ///     <see langword="null" />.
    /// </exception>
    public DisplayFieldTagHelper(ICompositeViewEngine viewEngine, IOptions<IconOptions> iconOptions)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _iconOptions = iconOptions ?? throw new ArgumentNullException(nameof(iconOptions));
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The field has neither <see cref="Label" /> nor <see cref="For" />, a span or start
    ///     isn't valid, or the display template isn't found.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Razor writes a string attribute bound to a null expression as an empty string, so an
        // empty value means "not set" and the metadata defaults still apply.
        EmptyText = string.IsNullOrEmpty(EmptyText) ? null : EmptyText;
        Format = string.IsNullOrEmpty(Format) ? null : Format;
        Href = string.IsNullOrEmpty(Href) ? null : Href;
        Secondary = string.IsNullOrEmpty(Secondary) ? null : Secondary;
        Template = string.IsNullOrEmpty(Template) ? null : Template;

        var label = string.IsNullOrEmpty(Label) ? For?.Metadata.GetDisplayName() : Label;
        if (label is null)
        {
            throw new InvalidOperationException(
                "An <sa-display-field> requires a label or asp-for attribute."
            );
        }

        // In a details list the field is a grid cell holding a dt and dd; directly in an sa-grid
        // it is wrapped in a grid item; elsewhere, such as among inputs in a form section, a dt
        // outside a dl is invalid, so both are divs.
        var grid = GetEnclosingGrid(context);
        var details = grid?.Owner as DetailsTagHelper;
        var placementStyle = GridLayout.PlacementStyle(grid, "sa-display-field", this);
        var effectiveOrientation =
            Orientation
            ?? (
                details?.Orientation == DetailsOrientation.Horizontal
                    ? DisplayFieldOrientation.Horizontal
                    : DisplayFieldOrientation.Vertical
            );

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "display-field");
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );
        if (details is not null && placementStyle is not null)
        {
            output.AppendStyle(placementStyle);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field", "sa-display-field", output.GetUserSuppliedClass())
        );

        var labelElement = new TagBuilder(details is null ? "div" : "dt");
        labelElement.Attributes["data-slot"] = "display-field-label";
        labelElement.AddCssClass("sa-display-field-label");
        labelElement.InnerHtml.Append(label);

        var valueElement = new TagBuilder(details is null ? "div" : "dd");
        valueElement.Attributes["data-slot"] = "display-field-value";
        valueElement.AddCssClass("sa-display-field-value");
        await RenderValueAsync(context, valueElement, await output.GetChildContentAsync());

        output.Content.SetHtmlContent(labelElement);
        output.Content.AppendHtml(valueElement);

        if (grid is not null && details is null)
        {
            GridLayout.WrapInItem(output, placementStyle);
        }
    }

    private static string? GetMetadataHref(object value, ModelMetadata metadata)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture);

        // Only web URLs become links, so a stored javascript: URL renders as text
        return metadata.DataTypeName switch
        {
            nameof(DataType.EmailAddress) => $"mailto:{text}",
            nameof(DataType.Url)
                when Uri.TryCreate(text, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) =>
                text,
            _ => null,
        };
    }

    private static bool IsEmpty(object? value)
    {
        return value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            IEnumerable items => !items.Cast<object?>().Any(),
            _ => false,
        };
    }

    private static bool IsNumber(Type type)
    {
        return Type.GetTypeCode(Nullable.GetUnderlyingType(type) ?? type)
            is TypeCode.Byte
                or TypeCode.SByte
                or TypeCode.Int16
                or TypeCode.UInt16
                or TypeCode.Int32
                or TypeCode.UInt32
                or TypeCode.Int64
                or TypeCode.UInt64
                or TypeCode.Single
                or TypeCode.Double
                or TypeCode.Decimal;
    }

    private string FormatItem(object? item, ModelMetadata metadata)
    {
        var format = Format ?? metadata.DisplayFormatString;
        if (format is null && item is Enum enumValue && metadata.IsEnum)
        {
            // Enum members show their [Display] names, as editors do
            var numericValue = enumValue.ToString("D");
            foreach (var (groupAndName, value) in metadata.EnumGroupedDisplayNamesAndValues!)
            {
                if (value == numericValue)
                {
                    return groupAndName.Name;
                }
            }
        }

        return format is null
            ? Convert.ToString(item, CultureInfo.CurrentCulture) ?? string.Empty
            : string.Format(CultureInfo.CurrentCulture, format, item);
    }

    private string FormatValue(object value, ModelMetadata metadata)
    {
        return value is IEnumerable items and not string
            ? string.Join(
                ", ",
                items
                    .Cast<object?>()
                    .Select(item => FormatItem(item, metadata.ElementMetadata ?? metadata))
            )
            : FormatItem(value, metadata);
    }

    private IHtmlContent RenderBoolean(TagHelperContext context, bool value)
    {
        var icon = new TagHelperOutput(
            "svg",
            [
                new TagHelperAttribute("class", "size-4"),
                new TagHelperAttribute("aria-hidden", "true"),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.Value.GetSemanticIconName(
                value ? SemanticIconRole.BooleanTrue : SemanticIconRole.BooleanFalse
            ),
        }.Process(context, icon);

        var text = new TagBuilder("span");
        text.AddCssClass("sr-only");
        text.InnerHtml.Append(value ? "Yes" : "No");

        return new HtmlContentBuilder().AppendHtml(icon).AppendHtml(text);
    }

    private async Task<IHtmlContent> RenderTemplateAsync(string templateName)
    {
        var viewName =
            templateName.StartsWith('/') || templateName.StartsWith("~/", StringComparison.Ordinal)
                ? templateName
                : $"DisplayTemplates/{templateName}";

        // Templates resolve by name only, never by type, so the framework's built-in display
        // templates (a bool as a disabled checkbox) never apply. A missing template throws
        // rather than falling back, so a mistyped name is caught.
        var getViewResult = _viewEngine.GetView(
            ViewContext.ExecutingFilePath,
            viewName,
            isMainPage: false
        );
        var result = getViewResult.Success
            ? getViewResult
            : _viewEngine.FindView(ViewContext, viewName, isMainPage: false);
        if (!result.Success)
        {
            var searchedLocations = getViewResult
                .SearchedLocations.Concat(result.SearchedLocations)
                .Distinct();
            throw new InvalidOperationException(
                $"The display template '{viewName}' was not found. The following locations "
                    + $"were searched: {string.Join(", ", searchedLocations)}."
            );
        }

        var viewData = new ViewDataDictionary<object?>(ViewContext.ViewData, For!.Model)
        {
            ModelExplorer = For.ModelExplorer,
        };
        await using var writer = new StringWriter();
        await result.View.RenderAsync(new ViewContext(ViewContext, result.View, viewData, writer));

        return new HtmlString(writer.ToString());
    }

    private async Task RenderValueAsync(
        TagHelperContext context,
        TagBuilder valueElement,
        TagHelperContent childContent
    )
    {
        var metadata = For?.Metadata;
        var value = For?.Model;
        var templateName = Template ?? metadata?.TemplateHint;

        if (!childContent.IsEmptyOrWhiteSpace)
        {
            valueElement.InnerHtml.AppendHtml(childContent);
        }
        else if (
            metadata is null
            || (IsEmpty(value) && !((TemplateRendersEmpty ?? false) && templateName is not null))
        )
        {
            valueElement.Attributes["data-empty"] = "true";
            valueElement.InnerHtml.Append(
                EmptyText ?? metadata?.NullDisplayText ?? DefaultEmptyText
            );

            return;
        }
        else if (templateName is not null)
        {
            valueElement.InnerHtml.AppendHtml(await RenderTemplateAsync(templateName));
        }
        else if (value is bool booleanValue)
        {
            valueElement.Attributes["data-kind"] = "boolean";
            valueElement.Attributes["data-boolean"] = booleanValue ? "true" : "false";
            valueElement.InnerHtml.AppendHtml(RenderBoolean(context, booleanValue));
        }
        else
        {
            var text = FormatValue(value!, metadata);
            if (IsNumber(metadata.ModelType))
            {
                valueElement.Attributes["data-kind"] = "number";
            }
            else if (metadata.DataTypeName == nameof(DataType.MultilineText))
            {
                valueElement.Attributes["data-kind"] = "text";
            }

            var href = Href ?? GetMetadataHref(value!, metadata);
            if (href is null)
            {
                valueElement.InnerHtml.Append(text);
            }
            else
            {
                var link = new TagBuilder("a");
                link.Attributes["href"] = href;
                link.InnerHtml.Append(text);
                valueElement.InnerHtml.AppendHtml(link);
            }
        }

        if (Secondary is not null)
        {
            var secondary = new TagBuilder("span");
            secondary.Attributes["data-slot"] = "display-field-secondary";
            secondary.AddCssClass("sa-display-field-secondary");
            secondary.InnerHtml.Append(Secondary);
            valueElement.InnerHtml.AppendHtml(secondary);
        }
    }
}
