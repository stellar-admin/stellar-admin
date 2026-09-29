using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Base class for StellarAdmin.TagHelpers form-input tag helpers. Wires up <c>asp-for</c> model binding and
///     the shared field attributes (label, description, error) that wrap an input control.
/// </summary>
public abstract class FieldInputBaseTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private string? _descriptionId;
    private string? _errorId;

    protected FieldInputBaseTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     Supporting help text rendered as the field's description.
    /// </summary>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     An error message rendered for the field.
    /// </summary>
    [HtmlAttributeName("error")]
    public string? Error { get; set; }

    /// <summary>
    ///     An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     The text rendered as the field's label.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    protected virtual FieldClassNames? FieldClasses => null;

    /// <summary>
    ///     The <c>id</c> of the element the automatically rendered label targets. Set by default
    ///     from the host's <c>id</c> when the input is not model-bound; derived classes may
    ///     override it when the labelled control gets a different id.
    /// </summary>
    protected string? LabelForId { get; set; }

    // When set, the automatically rendered label gets this id and no for attribute, so a
    // non-labelable control (such as a choice group) can reference it with aria-labelledby.
    private protected string? LabelId { get; set; }

    /// <summary>
    ///     The name of the &lt;input&gt; element.
    /// </summary>
    /// <remarks>
    ///     Passed through to the generated HTML in all cases. Also used to determine whether <see cref="For" /> is
    ///     valid with an empty <see cref="ModelExpression.Name" />.
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     Whether to wrap the input in a field along with its label, description, and error. When not set,
    ///     a field is rendered automatically if a label, description, error, or <see cref="For" /> is supplied
    ///     and the input is not already nested inside a field.
    /// </summary>
    [HtmlAttributeName("render-field")]
    public bool? ShouldRenderField { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var fieldLayout = await InternalRenderInput(context, output);

        if (ShouldRenderFieldWrapper())
        {
            await RenderFieldWrapper(context, output, fieldLayout);
        }
    }

    protected abstract Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    );

    private async Task<FieldLayout> InternalRenderInput(
        TagHelperContext context,
        TagHelperOutput output
    )
    {
        if (Name != null)
        {
            output.CopyHtmlAttribute(nameof(Name), context);
        }

        // Without asp-for the framework generates no id, so an implicit label would have nothing
        // to point at: mint one from the tag's unique id (unless the author supplied one) and
        // target it from the label. Model-bound inputs get their id and label from the framework.
        if (For == null)
        {
            if (
                Label != null
                && !output.Attributes.ContainsName("id")
                && ShouldRenderFieldWrapper()
            )
            {
                output.Attributes.SetAttribute("id", $"sa-{GetUniqueId(context)}");
            }

            LabelForId = output.Attributes["id"]?.Value?.ToString();
        }

        IDictionary<string, object?>? htmlAttributes = null;
        if (
            string.IsNullOrEmpty(For?.Name)
            && string.IsNullOrEmpty(ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix)
            && !string.IsNullOrEmpty(Name)
        )
        {
            htmlAttributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "name", Name },
            };
        }

        return await RenderInput(context, output, htmlAttributes);
    }

    private async Task RenderDescriptionControl(
        TagHelperContext context,
        TagHelperContent targetContent
    )
    {
        if (For != null || Description != null)
        {
            var descriptionTagHelperOutput = new TagHelperOutput(
                string.Empty,
                _descriptionId == null
                    ? [new TagHelperAttribute("class", FieldClasses?.Description ?? string.Empty)]
                    :
                    [
                        new TagHelperAttribute("id", _descriptionId),
                        new TagHelperAttribute("class", FieldClasses?.Description ?? string.Empty),
                    ],
                (_, _) =>
                    Description == null
                        ? Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                        : Task.FromResult(new DefaultTagHelperContent().Append(Description))
            );
            var fieldDescriptionTagHelper = new FieldDescriptionTagHelper()
            {
                For = For,
                ViewContext = ViewContext,
            };
            await fieldDescriptionTagHelper.ProcessAsync(context, descriptionTagHelperOutput);

            targetContent.AppendHtml(descriptionTagHelperOutput);
        }
    }

    private async Task RenderFieldPart(
        TagHelperContext context,
        TagHelperContent targetContent,
        FieldPart part
    )
    {
        switch (part)
        {
            case FieldPart.Label:
                await RenderLabelControl(context, targetContent);
                break;
            case FieldPart.Description:
                await RenderDescriptionControl(context, targetContent);
                break;
            case FieldPart.Error:
                await RenderErrorControl(context, targetContent);
                break;
        }
    }

    private async Task RenderFieldParts(
        TagHelperContext context,
        TagHelperContent targetContent,
        IReadOnlyList<FieldPart> parts,
        FieldOrientation orientation
    )
    {
        if (parts.Count == 0)
        {
            return;
        }

        if (orientation == FieldOrientation.Vertical)
        {
            foreach (var part in parts)
            {
                await RenderFieldPart(context, targetContent, part);
            }

            return;
        }

        // Beside the control, the parts are kept stacked together in a field content container
        var fieldContentOutput = new TagHelperOutput(
            string.Empty,
            [new TagHelperAttribute("class", FieldClasses?.Content ?? string.Empty)],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var fieldContentTagHelper = new FieldContentTagHelper();
        await fieldContentTagHelper.ProcessAsync(context, fieldContentOutput);

        foreach (var part in parts)
        {
            await RenderFieldPart(context, fieldContentOutput.Content, part);
        }

        targetContent.AppendHtml(fieldContentOutput);
    }

    private async Task RenderFieldWrapper(
        TagHelperContext context,
        TagHelperOutput output,
        FieldLayout fieldLayout
    )
    {
        var fieldTagBuilder = new FieldTagBuilder(fieldLayout.Orientation, FieldClasses?.Root);

        output.PreElement.AppendHtml(fieldTagBuilder.RenderStartTag());
        await RenderFieldParts(
            context,
            output.PreElement,
            fieldLayout.BeforeControl,
            fieldLayout.Orientation
        );
        await RenderFieldParts(
            context,
            output.PostElement,
            fieldLayout.AfterControl,
            fieldLayout.Orientation
        );
        output.PostElement.AppendHtml(fieldTagBuilder.RenderEndTag());
    }

    private async Task RenderErrorControl(TagHelperContext context, TagHelperContent targetContent)
    {
        if (For != null || Error != null)
        {
            var errorTagHelperOutput = new TagHelperOutput(
                string.Empty,
                _errorId == null
                    ? [new TagHelperAttribute("class", FieldClasses?.Error ?? string.Empty)]
                    :
                    [
                        new TagHelperAttribute("id", _errorId),
                        new TagHelperAttribute("class", FieldClasses?.Error ?? string.Empty),
                    ],
                (_, _) =>
                    Error == null
                        ? Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                        : Task.FromResult(new DefaultTagHelperContent().Append(Error))
            );
            var fieldErrorTagHelper = new FieldErrorTagHelper(_htmlGenerator)
            {
                For = For,
                ViewContext = ViewContext,
            };
            await fieldErrorTagHelper.ProcessAsync(context, errorTagHelperOutput);

            targetContent.AppendHtml(errorTagHelperOutput);
        }
    }

    private async Task RenderLabelControl(TagHelperContext context, TagHelperContent targetContent)
    {
        if (For != null || Label != null)
        {
            var labelTagHelperOutput = new TagHelperOutput(
                string.Empty,
                LabelId != null
                        ?
                        [
                            new TagHelperAttribute("id", LabelId),
                            new TagHelperAttribute("class", FieldClasses?.Label ?? string.Empty),
                        ]
                    : LabelForId == null
                        ? [new TagHelperAttribute("class", FieldClasses?.Label ?? string.Empty)]
                    :
                    [
                        new TagHelperAttribute("for", LabelForId),
                        new TagHelperAttribute("class", FieldClasses?.Label ?? string.Empty),
                    ],
                (_, _) =>
                    Label == null
                        ? Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                        : Task.FromResult(new DefaultTagHelperContent().Append(Label))
            );
            var fieldLabelTagHelper = new FieldLabelTagHelper(_htmlGenerator)
            {
                For = For,
                ViewContext = ViewContext,
            };
            await fieldLabelTagHelper.ProcessAsync(context, labelTagHelperOutput);
            if (LabelId != null)
            {
                // The framework label adds a for attribute from asp-for
                labelTagHelperOutput.Attributes.RemoveAll("for");
            }

            targetContent.AppendHtml(labelTagHelperOutput);
        }
    }

    // Points the control at the field's rendered description and error, after any ids the author
    // supplied, and marks it invalid when the field has an error unless the author set a value.
    private protected void ApplyFieldAttributes(
        TagHelperContext context,
        TagHelperAttributeList attributes,
        FieldLayout layout
    )
    {
        if (!attributes.ContainsName("aria-invalid") && IsInvalid())
        {
            attributes.SetAttribute("aria-invalid", "true");
        }

        if (!ShouldRenderFieldWrapper())
        {
            return;
        }

        var describedBy = attributes["aria-describedby"]?.Value?.ToString();
        if (
            HasFieldPart(layout, FieldPart.Description)
            && (
                !string.IsNullOrWhiteSpace(Description)
                || For?.Metadata.Description is { Length: > 0 }
            )
        )
        {
            _descriptionId = $"sa-{GetUniqueId(context)}-description";
            describedBy = JoinCssClasses(describedBy, _descriptionId);
        }

        if (HasFieldPart(layout, FieldPart.Error) && (For != null || Error != null))
        {
            _errorId = $"sa-{GetUniqueId(context)}-error";
            describedBy = JoinCssClasses(describedBy, _errorId);
        }

        if (!string.IsNullOrEmpty(describedBy))
        {
            attributes.SetAttribute("aria-describedby", describedBy);
        }
    }

    private protected bool IsInvalid() =>
        !string.IsNullOrEmpty(Error)
        || For != null
            && ViewContext.ViewData.ModelState.TryGetValue(
                ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name),
                out var entry
            )
            && entry.Errors.Count > 0;

    private static bool HasFieldPart(FieldLayout layout, FieldPart part) =>
        layout.BeforeControl.Contains(part) || layout.AfterControl.Contains(part);

    private protected bool WillRenderFieldLabel() =>
        ShouldRenderFieldWrapper() && (For != null || Label != null);

    private bool ShouldRenderFieldWrapper()
    {
        // It the user explicitly indicated whether we should render a field, then we honor that
        if (ShouldRenderField.HasValue)
        {
            return ShouldRenderField.Value;
        }

        // If we're rendering inside a field tag helper, we don't render another one
        if (GetParentTagHelper<FieldTagHelper>() != null)
        {
            return false;
        }

        // If any of the explicit field attributes or the asp-for attribute is set, we will render a field
        if (For != null || Label != null || Description != null || Error != null)
        {
            return true;
        }

        // If none of the above conditions are true, do not render a field
        return false;
    }
}
