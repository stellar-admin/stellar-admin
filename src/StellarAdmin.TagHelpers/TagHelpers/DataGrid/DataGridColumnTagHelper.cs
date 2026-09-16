using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Defines a column of a data grid. The cell value comes from, in order of precedence: a
///     nested <c>sa-data-grid-item-template</c>, plain child content (re-rendered per row), or
///     the property named by <c>field</c> or selected by <c>field-for</c>. A field cell
///     renders through a grid display template when the column declares a <c>template</c> or
///     the field property carries a <c>[UIHint]</c>. The header renders a nested
///     <c>sa-data-grid-header-template</c> when present, otherwise <c>title</c>, otherwise a
///     field column derives its header from the field's display name. A <c>class</c>
///     attribute is applied to both the header and body cells of the column.
/// </summary>
[HtmlTargetElement("sa-data-grid-column", ParentTag = "sa-data-grid")]
public class DataGridColumnTagHelper : StellarAdminTagHelperBase
{
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly ICompositeViewEngine _viewEngine;

    /// <summary>
    ///     The name of a public property on the row item whose value renders in the column's
    ///     cells, or a dotted path to a nested property (<c>Customer.Name</c>; a null
    ///     intermediate value renders as a null field value). Ignored when the column has
    ///     template or inline content. Mutually exclusive with <c>field-for</c>.
    /// </summary>
    [HtmlAttributeName("field")]
    public string? Field { get; set; }

    /// <summary>
    ///     A lambda expression selecting the public property on the row item whose value
    ///     renders in the column's cells — the typed alternative to <c>field</c>, with IDE
    ///     completion and compile-time checking. The expression must select a property or a
    ///     chain of properties (<c>b =&gt; b.Customer.Name</c>; a null intermediate value
    ///     renders as a null field value), and in markup the lambda parameter needs an
    ///     explicit type: <c>field-for="(Booking b) =&gt; b.Reference"</c>. All
    ///     <c>field</c> behavior (templates, formats, metadata, sort defaulting) applies
    ///     identically, except that field metadata resolves from the lambda parameter type
    ///     instead of the runtime item type, so <c>[UIHint]</c> templates and display names
    ///     also work on an empty grid. Mutually exclusive with <c>field</c>.
    /// </summary>
    [HtmlAttributeName("field-for")]
    public LambdaExpression? FieldFor { get; set; }

    /// <summary>
    ///     A composite format string (e.g. <c>{0:C}</c>) applied to the field value.
    ///     Defaults to the field's <c>[DisplayFormat]</c> format string when omitted. Ignored
    ///     when the cell renders through a grid display template.
    /// </summary>
    [HtmlAttributeName("format")]
    public string? Format { get; set; }

    /// <summary>
    ///     Whether the column header renders a sort link. Requires an
    ///     <c>sa-data-grid-sort</c> child on the grid declaring the sort URL.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("sortable")]
    public bool? Sortable { get; set; }

    /// <summary>
    ///     The sort field substituted for the <c>{sort}</c> placeholder in the column's sort
    ///     links, when it differs from the column's field.
    /// </summary>
    [HtmlAttributeName("sort-field")]
    public string? SortField { get; set; }

    /// <summary>
    ///     The name of the grid display template that renders the column's cells, with the
    ///     field value as the template's model. The name resolves like a partial view
    ///     named <c>GridDisplayTemplates/{template}</c> (e.g.
    ///     <c>/Views/Shared/GridDisplayTemplates/</c> or
    ///     <c>/Pages/Shared/GridDisplayTemplates/</c>); an application-relative path
    ///     (<c>~/</c> or <c>/</c>) is used verbatim. Takes precedence over a <c>[UIHint]</c>
    ///     on the field property. Requires <c>field</c> or <c>field-for</c>.
    /// </summary>
    [HtmlAttributeName("template")]
    public string? Template { get; set; }

    /// <summary>
    ///     The column header text. When omitted on a field column, the header defaults to the
    ///     field's display name (<c>[Display(Name = ...)]</c>, honoring metadata classes) and
    ///     then to the property name split on capitals (<c>EmailConfirmed</c> → "Email
    ///     Confirmed"). A nested <c>sa-data-grid-header-template</c> takes precedence over
    ///     all of these.
    /// </summary>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public DataGridColumnTagHelper(
        ICompositeViewEngine viewEngine,
        IModelMetadataProvider metadataProvider
    )
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _metadataProvider =
            metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is null)
        {
            output.SuppressOutput();
            return;
        }

        if (Field is not null && FieldFor is not null)
        {
            throw new InvalidOperationException(
                "An <sa-data-grid-column> cannot have both a field and a field-for attribute."
            );
        }

        // Razor writes a string-bound attribute whose value is a null expression as an
        // empty string (title="@column.Title" with a null title), so an empty value means
        // "not set" — normalize to null so the metadata-based defaults still apply.
        Title = string.IsNullOrEmpty(Title) ? null : Title;
        Template = string.IsNullOrEmpty(Template) ? null : Template;
        Format = string.IsNullOrEmpty(Format) ? null : Format;

        var fieldPropertyChain = FieldFor is null ? null : ExtractPropertyChain(FieldFor);
        var fieldName =
            Field
            ?? (
                fieldPropertyChain is { } chain
                    ? string.Join('.', chain.Select(property => property.Name))
                    : null
            );

        var columnContext = new DataGridColumnContext { CurrentRow = gridContext.CurrentRow };
        SetContext(context, columnContext);

        var childContent = await output.GetChildContentAsync(useCachedResult: false);

        // Initial column processing
        if (gridContext.CurrentRow is not { } currentRow)
        {
            var effectiveSortable = Sortable ?? false;
            var sortField = SortField ?? fieldName;
            if (effectiveSortable && sortField is null)
            {
                throw new InvalidOperationException(
                    "A sortable <sa-data-grid-column> requires a field, field-for, or "
                        + "sort-field attribute."
                );
            }

            if (Template is not null && fieldName is null)
            {
                throw new InvalidOperationException(
                    "An <sa-data-grid-column> with a template attribute requires a field or "
                        + "field-for attribute naming the property that provides the "
                        + "template's model."
                );
            }

            // Resolve field metadata and the display template now, while collecting, so
            // their lookup cost is paid once per column rather than once per row.
            var fieldMetadata = GetFieldMetadata(gridContext, fieldName);
            if ((Template ?? fieldMetadata?.TemplateHint) is { } templateName)
            {
                ResolveDisplayTemplate(gridContext, templateName);
            }

            gridContext.Columns.Add(
                new DataGridColumn
                {
                    // A dotted path defaults its header from the leaf property name.
                    Title =
                        Title
                        ?? fieldMetadata?.DisplayName
                        ?? (
                            fieldName is { } name
                                ? SplitPascalCase(name[(name.LastIndexOf('.') + 1)..])
                                : null
                        ),
                    HeaderHtml = columnContext.HeaderContent,
                    CssClass = output.GetUserSuppliedClass() is { Length: > 0 } cssClass
                        ? cssClass
                        : null,
                    Sortable = effectiveSortable,
                    SortField = sortField,
                }
            );

            output.SuppressOutput();
            return;
        }

        string cellHtml;
        if (columnContext.ItemContent is not null)
        {
            cellHtml = columnContext.ItemContent;
        }
        else if (!childContent.IsEmptyOrWhiteSpace)
        {
            cellHtml = childContent.GetContent();
        }
        else if (fieldName is not null)
        {
            var value = currentRow.Item is { } item
                ? GetFieldValue(item, fieldName, fieldPropertyChain)
                : null;

            var fieldMetadata = GetFieldMetadata(gridContext, fieldName);
            if ((Template ?? fieldMetadata?.TemplateHint) is { } templateName)
            {
                cellHtml = await RenderTemplate(
                    ResolveDisplayTemplate(gridContext, templateName),
                    value
                );
            }
            else
            {
                var format = Format ?? fieldMetadata?.DisplayFormatString;
                cellHtml = HtmlEncoder.Default.Encode(
                    format is not null ? string.Format(format, value) : value?.ToString() ?? ""
                );
            }
        }
        else
        {
            cellHtml = "";
        }

        currentRow.Cells.Add(
            new DataGridCell(
                output.GetUserSuppliedClass() is { Length: > 0 } cellCssClass ? cellCssClass : null,
                cellHtml
            )
        );
        output.SuppressOutput();
    }

    /// <summary>
    ///     Extracts the property chain selected by a <c>field-for</c> expression,
    ///     root-first (<c>b =&gt; b.Customer.Name</c> yields <c>[Customer, Name]</c>).
    ///     Every link must be a property access and the chain must start at the lambda
    ///     parameter — anything else (method calls, indexers, fields, casts mid-chain) is
    ///     unsupported. A boxing <c>Convert</c> node (from an expression built as
    ///     <c>Func&lt;T, object&gt;</c>) is unwrapped first.
    /// </summary>
    private static PropertyInfo[] ExtractPropertyChain(LambdaExpression fieldFor)
    {
        var body = fieldFor.Body;
        if (
            body is UnaryExpression
            {
                NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked
            } convert
        )
        {
            body = convert.Operand;
        }

        var chain = new List<PropertyInfo>();
        while (body is MemberExpression { Member: PropertyInfo property, Expression: { } instance })
        {
            chain.Add(property);
            body = instance;
        }

        if (body is ParameterExpression && chain.Count > 0)
        {
            chain.Reverse();
            return chain.ToArray();
        }

        throw new InvalidOperationException(
            $"The <sa-data-grid-column> field-for expression '{fieldFor}' is not supported. "
                + "Only a property or chain of properties selected on the row item is "
                + "supported, like (Booking b) => b.Customer.Name."
        );
    }

    private static string SplitPascalCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (
                i > 0
                && char.IsUpper(name[i])
                && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1])))
            )
            {
                builder.Append(' ');
            }

            builder.Append(name[i]);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Returns the field's display metadata (the source of <c>[UIHint]</c> template hints,
    ///     <c>[DisplayFormat]</c> format strings, and <c>[Display]</c> names), resolved
    ///     through the framework's model metadata pipeline so metadata classes attached with
    ///     <c>[ModelMetadataType]</c> and custom metadata providers are honored. A dotted
    ///     path resolves segment by segment — each hop goes through the pipeline, and the
    ///     leaf property's metadata is returned. Expression columns resolve from the lambda
    ///     parameter type (so metadata works on an empty grid); string fields resolve from
    ///     the runtime item type sniffed by the grid. Resolved once per field on the grid
    ///     context; <c>null</c> when no source type is available.
    /// </summary>
    private ModelMetadata? GetFieldMetadata(DataGridContext gridContext, string? fieldName)
    {
        if (fieldName is null)
        {
            return null;
        }

        if (!gridContext.FieldMetadata.TryGetValue(fieldName, out var fieldMetadata))
        {
            var sourceType = FieldFor?.Parameters[0].Type ?? gridContext.ItemType;
            if (sourceType is { } containerType)
            {
                ModelMetadata? metadata = null;
                foreach (var segment in fieldName.Split('.'))
                {
                    metadata = _metadataProvider.GetMetadataForProperty(containerType, segment);
                    containerType = metadata.ModelType;
                }

                fieldMetadata = metadata;
            }

            gridContext.FieldMetadata[fieldName] = fieldMetadata;
        }

        return fieldMetadata;
    }

    /// <summary>
    ///     Reads the column's field value off a row item through the shared getter cache in
    ///     <see cref="DataGridFieldGetters" />. String fields resolve against the item's
    ///     runtime type, so each concrete type in a polymorphic list gets its own getter;
    ///     expression fields resolve against the lambda's declared parameter type, so the
    ///     getter matches the C# semantics of the expression (explicit interface
    ///     implementations, shadowed properties).
    /// </summary>
    private object? GetFieldValue(
        object item,
        string fieldName,
        IReadOnlyList<PropertyInfo>? fieldPropertyChain
    )
    {
        return FieldFor is null
            ? DataGridFieldGetters.GetValue(item, fieldName)
            : DataGridFieldGetters.GetValue(
                item,
                FieldFor.Parameters[0].Type,
                fieldName,
                fieldPropertyChain!
            );
    }

    private async Task<string> RenderTemplate(IView view, object? model)
    {
        // Model is assigned explicitly so a null field value reaches the template as null
        // instead of the view data falling back to the page's model.
        var viewData = new ViewDataDictionary<object?>(ViewContext.ViewData, model)
        {
            Model = model,
        };
        using var writer = new StringWriter();
        await view.RenderAsync(new ViewContext(ViewContext, view, viewData, writer));
        return writer.ToString();
    }

    private IView ResolveDisplayTemplate(DataGridContext gridContext, string templateName)
    {
        if (gridContext.GridDisplayTemplates.TryGetValue(templateName, out var resolvedView))
        {
            return resolvedView;
        }

        var viewName =
            templateName.StartsWith('/') || templateName.StartsWith("~/", StringComparison.Ordinal)
                ? templateName
                : $"GridDisplayTemplates/{templateName}";

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
                $"The grid display template '{viewName}' was not found. The following "
                    + $"locations were searched: {string.Join(", ", searchedLocations)}."
            );
        }

        gridContext.GridDisplayTemplates[templateName] = result.View;
        return result.View;
    }
}
