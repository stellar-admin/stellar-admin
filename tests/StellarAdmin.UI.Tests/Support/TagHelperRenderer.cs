using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.Tests.Support;

internal static class TagHelperRenderer
{
    public static async Task<IHtmlDocument> RenderAsync(
        TagHelper sut,
        Func<TagHelperContext, Task<string>>? children = null,
        IEnumerable<TagHelperAttribute>? outputAttributes = null
    )
    {
        return new HtmlParser().ParseDocument(
            await RenderToStringAsync(sut, children, outputAttributes)
        );
    }

    /// <summary>
    ///     Renders a tag helper that outputs table rows inside a table body, since an HTML parser
    ///     drops a row found outside a table.
    /// </summary>
    public static async Task<IHtmlDocument> RenderInTableBodyAsync(
        TagHelper sut,
        Func<TagHelperContext, Task<string>>? children = null,
        IEnumerable<TagHelperAttribute>? outputAttributes = null
    )
    {
        var rows = await RenderToStringAsync(sut, children, outputAttributes);

        return new HtmlParser().ParseDocument($"<table><tbody>{rows}</tbody></table>");
    }

    private static async Task<string> RenderToStringAsync(
        TagHelper sut,
        Func<TagHelperContext, Task<string>>? children,
        IEnumerable<TagHelperAttribute>? outputAttributes
    )
    {
        var attributes = new TagHelperAttributeList();
        if (sut is InputTagHelper { InputTypeName: not null } input)
        {
            attributes.Add("type", input.InputTypeName);
        }

        if (sut is InputTagHelper { Value: not null } valuedInput)
        {
            attributes.Add("value", valuedInput.Value);
        }

        if (sut is FieldInputBaseTagHelper { Name: not null } fieldInput)
        {
            attributes.Add("name", fieldInput.Name);
        }

        var context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString()
        );
        sut.Init(context);
        var output = new TagHelperOutput(
            "sa-test",
            new TagHelperAttributeList([
                new TagHelperAttribute("class", "existing-class"),
                .. outputAttributes ?? [],
            ]),
            async (_, _) =>
                new DefaultTagHelperContent().SetHtmlContent(
                    children == null ? "" : await children(context)
                )
        );
        await sut.ProcessAsync(context, output);

        // As Razor does, render the children when the tag helper left its content untouched.
        if (!output.IsContentModified)
        {
            output.Content.SetHtmlContent(await output.GetChildContentAsync());
        }

        return Serialize(output);
    }

    public static async Task<string> RenderChildAsync(
        TagHelper sut,
        TagHelperContext parent,
        Func<TagHelperContext, Task<string>>? children = null,
        IEnumerable<TagHelperAttribute>? outputAttributes = null
    )
    {
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(parent.Items),
            Guid.NewGuid().ToString()
        );
        sut.Init(context);
        var output = new TagHelperOutput(
            "sa-test",
            new TagHelperAttributeList([
                new TagHelperAttribute("class", "existing-class"),
                .. outputAttributes ?? [],
            ]),
            async (_, _) =>
                new DefaultTagHelperContent().SetHtmlContent(
                    children == null ? "" : await children(context)
                )
        );
        await sut.ProcessAsync(context, output);

        return Serialize(output);
    }

    /// <summary>
    ///     Renders a table cell tag helper inside a table row, since an HTML parser drops a cell
    ///     found outside a table.
    /// </summary>
    public static async Task<IHtmlDocument> RenderInTableRowAsync(
        TagHelper sut,
        IEnumerable<TagHelperAttribute>? outputAttributes = null
    )
    {
        var parent = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString()
        );
        var cell = await RenderChildAsync(sut, parent, outputAttributes: outputAttributes);

        return new HtmlParser().ParseDocument($"<table><tbody><tr>{cell}</tr></tbody></table>");
    }

    private static string Serialize(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
