using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers.Tests.Support;

internal static class TagHelperRenderer
{
    public static async Task<IHtmlDocument> RenderAsync(
        TagHelper sut,
        Func<TagHelperContext, Task<string>>? children = null
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
            new TagHelperAttributeList { { "class", "existing-class" } },
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

        return new HtmlParser().ParseDocument(Serialize(output));
    }

    public static async Task<string> RenderChildAsync(TagHelper sut, TagHelperContext parent)
    {
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(parent.Items),
            Guid.NewGuid().ToString()
        );
        sut.Init(context);
        var output = new TagHelperOutput(
            "sa-test",
            new TagHelperAttributeList { { "class", "existing-class" } },
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await sut.ProcessAsync(context, output);

        return Serialize(output);
    }

    private static string Serialize(TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
