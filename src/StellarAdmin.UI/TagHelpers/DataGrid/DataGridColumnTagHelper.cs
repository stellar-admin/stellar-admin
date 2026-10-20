using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Defines a column of a data grid.
/// </summary>
/// <remarks>
///     <para>
///         A cell shows the first of these that the column has: an
///         <c>sa-data-grid-item-template</c>, its content, or the value of <c>field</c> or
///         <c>field-for</c>. A field value renders through a grid display template when the
///         column sets <c>template</c> or the property has a <c>[UIHint]</c>.
///     </para>
///     <para>
///         The <c>class</c> applies to the column's header and body cells. In a row in edit mode,
///         an <c>sa-data-grid-edit-template</c> renders instead.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-data-grid-column", ParentTag = "sa-data-grid")]
public sealed class DataGridColumnTagHelper : StellarAdminTagHelperBase
{
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly ICompositeViewEngine _viewEngine;

    /// <summary>
    ///     Gets or sets the name of the item property whose value the cells show.
    /// </summary>
    /// <value>
    ///     A property name, or a dotted path to a nested property, such as <c>Customer.Name</c>.
    /// </value>
    /// <remarks>
    ///     Ignored when the column has an item template or content. Can't be combined with
    ///     <c>field-for</c>. A null value along the path shows as an empty value.
    /// </remarks>
    [HtmlAttributeName("field")]
    public string? Field { get; set; }

    /// <summary>
    ///     Gets or sets the text a cell shows when the field value is empty.
    /// </summary>
    /// <value>
    ///     The text for a null or blank value, or an empty collection. The default is the field's
    ///     null display text, or nothing when it has none.
    /// </value>
    /// <remarks>
    ///     The null display text comes from <c>[DisplayFormat(NullDisplayText)]</c>. Not used when
    ///     <c>template-renders-empty</c> passes empty values to the template.
    /// </remarks>
    [HtmlAttributeName("empty-text")]
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets an expression that selects the item property whose value the cells show.
    /// </summary>
    /// <value>
    ///     A lambda with an explicit parameter type that selects a property or a chain of
    ///     properties, such as <c>field-for="(Booking b) =&gt; b.Reference"</c>, or computes a
    ///     value, such as <c>field-for="(Booking b) =&gt; b.Nights * b.Rate"</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         A typed alternative to <c>field</c> that behaves the same way for a property
    ///         chain. Can't be combined with <c>field</c>. A null value along the chain shows as
    ///         an empty value.
    ///     </para>
    ///     <para>
    ///         The property's metadata comes from the parameter type, so <c>[UIHint]</c>
    ///         templates and display names also work on an empty grid.
    ///     </para>
    ///     <para>
    ///         A computed value has no property, so set <c>title</c>, and <c>sort-field</c> to
    ///         sort it. Reading a property or calling an instance method on a null value
    ///         doesn't throw: it reads as null, or as zero or false for a value type.
    ///     </para>
    /// </remarks>
    [HtmlAttributeName("field-for")]
    public LambdaExpression? FieldFor { get; set; }

    /// <summary>
    ///     Gets or sets the composite format string applied to the field value.
    /// </summary>
    /// <value>
    ///     A format string, such as <c>{0:C}</c>. The default is the field's display format.
    /// </value>
    /// <remarks>
    ///     The display format comes from <c>[DisplayFormat(DataFormatString)]</c>. Ignored when
    ///     the cell renders through a grid display template.
    /// </remarks>
    [HtmlAttributeName("format")]
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the column header renders a sort link.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the header renders a sort link; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Requires an <c>sa-data-grid-sort</c> on the grid, and <c>field</c>, <c>field-for</c> or
    ///     <c>sort-field</c> on the column.
    /// </remarks>
    [HtmlAttributeName("sortable")]
    public bool? Sortable { get; set; }

    /// <summary>
    ///     Gets or sets the value of the <c>{sortField}</c> placeholder in the column's sort links.
    /// </summary>
    /// <value>
    ///     The sort field. The default is the column's field name.
    /// </value>
    [HtmlAttributeName("sort-field")]
    public string? SortField { get; set; }

    /// <summary>
    ///     Gets or sets the name of the grid display template that renders the column's cells.
    /// </summary>
    /// <value>
    ///     A name that resolves like a partial view named <c>GridDisplayTemplates/{template}</c>,
    ///     such as one in <c>/Views/Shared/GridDisplayTemplates/</c>. A path that starts with
    ///     <c>~/</c> or <c>/</c> is used as is.
    /// </value>
    /// <remarks>
    ///     Requires <c>field</c> or <c>field-for</c>. The template's model is the field value, and
    ///     its metadata is the field property's, or the value type's for a computed
    ///     <c>field-for</c>. Takes precedence over a <c>[UIHint]</c> on the property.
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
    ///     Gets or sets the column header text.
    /// </summary>
    /// <value>
    ///     The header text. The default is the field's display name, or its property name split
    ///     into words, such as "Email Confirmed".
    /// </value>
    /// <remarks>
    ///     The display name comes from <c>[Display(Name)]</c>. An
    ///     <c>sa-data-grid-header-template</c> takes precedence.
    /// </remarks>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataGridColumnTagHelper" /> class.
    /// </summary>
    /// <param name="viewEngine">The view engine that finds grid display templates.</param>
    /// <param name="metadataProvider">The provider of the field properties' metadata.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="viewEngine" /> or <paramref name="metadataProvider" /> is
    ///     <see langword="null" />.
    /// </exception>
    public DataGridColumnTagHelper(
        ICompositeViewEngine viewEngine,
        IModelMetadataProvider metadataProvider
    )
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _metadataProvider =
            metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Field" /> and <see cref="FieldFor" /> are both set, a sortable column has no
    ///     field or sort field, <see cref="Template" /> is set without a field, or the grid display
    ///     template isn't found.
    /// </exception>
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
        EmptyText = string.IsNullOrEmpty(EmptyText) ? null : EmptyText;
        Template = string.IsNullOrEmpty(Template) ? null : Template;
        Format = string.IsNullOrEmpty(Format) ? null : Format;

        var fieldPropertyChain = FieldFor is null ? null : FindPropertyChain(FieldFor);
        var computed = FieldFor is not null && fieldPropertyChain is null;
        var fieldName =
            Field
            ?? (
                fieldPropertyChain is { } chain
                    ? string.Join('.', chain.Select(property => property.Name))
                    : null
            );

        // In a row pass, the column's definition is the one registered at the position of the
        // cell it is about to add.
        var editing =
            gridContext.CurrentRow is { Editing: true } row
            && gridContext.Columns.ElementAtOrDefault(row.Cells.Count) is { HasEditTemplate: true };
        var columnContext = new DataGridColumnContext
        {
            CurrentRow = gridContext.CurrentRow,
            Editing = editing,
        };
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

            if (Template is not null && fieldName is null && !computed)
            {
                throw new InvalidOperationException(
                    "An <sa-data-grid-column> with a template attribute requires a field or "
                        + "field-for attribute naming the property that provides the "
                        + "template's model."
                );
            }

            // Resolve field metadata and the display template now, while collecting, so
            // their lookup cost is paid once per column rather than once per row.
            var fieldMetadata = computed
                ? _metadataProvider.GetMetadataForType(FieldFor!.ReturnType)
                : GetFieldMetadata(gridContext, fieldName);
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
                    HasEditTemplate = columnContext.HasEditTemplate,
                    ComputedValue = computed ? ComputedValue.Compile(FieldFor!) : null,
                    ComputedMetadata = computed ? fieldMetadata : null,
                }
            );

            output.SuppressOutput();
            return;
        }

        string cellHtml;
        if (editing)
        {
            cellHtml = columnContext.EditContent ?? "";
        }
        else if (currentRow.IsNew)
        {
            // The new item need not be the grid's item type, so it has nothing to display.
            cellHtml = "";
        }
        else if (columnContext.ItemContent is not null)
        {
            cellHtml = columnContext.ItemContent;
        }
        else if (!childContent.IsEmptyOrWhiteSpace)
        {
            cellHtml = childContent.GetContent();
        }
        else if (fieldName is not null || computed)
        {
            // A computed column was compiled when it was collected, at the cell's position
            var column = computed ? gridContext.Columns[currentRow.Cells.Count] : null;
            var value =
                currentRow.Item is not { } item ? null
                : column is not null ? column.ComputedValue!(item)
                : GetFieldValue(item, fieldName!, fieldPropertyChain);

            var fieldMetadata = column is not null
                ? column.ComputedMetadata
                : GetFieldMetadata(gridContext, fieldName);
            var templateName = Template ?? fieldMetadata?.TemplateHint;
            if (IsEmpty(value) && !((TemplateRendersEmpty ?? false) && templateName is not null))
            {
                cellHtml = HtmlEncoder.Default.Encode(
                    EmptyText ?? fieldMetadata?.NullDisplayText ?? ""
                );
            }
            else if (templateName is not null)
            {
                cellHtml = await RenderTemplate(
                    ResolveDisplayTemplate(gridContext, templateName),
                    value,
                    fieldMetadata
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
    ///     Finds the property chain selected by a <c>field-for</c> expression,
    ///     root-first (<c>b =&gt; b.Customer.Name</c> yields <c>[Customer, Name]</c>).
    ///     Every link must be a property access and the chain must start at the lambda
    ///     parameter; anything else (method calls, operators, fields, casts mid-chain) is a
    ///     computed value, for which this returns <c>null</c>. A boxing <c>Convert</c> node
    ///     (from an expression built as <c>Func&lt;T, object&gt;</c>) is unwrapped first.
    /// </summary>
    private static PropertyInfo[]? FindPropertyChain(LambdaExpression fieldFor)
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

        if (body is not ParameterExpression || chain.Count == 0)
        {
            return null;
        }

        chain.Reverse();
        return chain.ToArray();
    }

    private static bool IsEmpty([NotNullWhen(false)] object? value)
    {
        return value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            IEnumerable items => !items.Cast<object?>().Any(),
            _ => false,
        };
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

    private async Task<string> RenderTemplate(
        IView view,
        object? value,
        ModelMetadata? fieldMetadata
    )
    {
        // The template sees the field property's metadata, as a display template does, so it
        // can read the property's [DisplayFormat], [DataType] and enum display names.
        var viewData = new ViewDataDictionary<object?>(ViewContext.ViewData, value);
        if (fieldMetadata is not null)
        {
            viewData.ModelExplorer = new ModelExplorer(_metadataProvider, fieldMetadata, value);
        }

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
