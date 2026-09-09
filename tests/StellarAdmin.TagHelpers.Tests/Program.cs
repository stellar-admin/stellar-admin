using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

var builder = WebApplication.CreateBuilder();
var services = builder.Services;
services.AddLogging();
services.AddMvc();
services.AddStellarAdmin().AddTagHelpers();

using var provider = services.BuildServiceProvider();
var generator = provider.GetRequiredService<IHtmlGenerator>();
var icons = provider.GetRequiredService<IIconManager>();
var metadata = provider.GetRequiredService<IModelMetadataProvider>();
var viewContext = new ViewContext
{
    HttpContext = new DefaultHttpContext { RequestServices = provider },
    ViewData = new ViewDataDictionary(metadata, new ModelStateDictionary()),
    RouteData = new RouteData(),
    ActionDescriptor = new ActionDescriptor(),
    FormContext = new FormContext(),
};

var inputClasses = new InputClassNames
{
    Root = "custom-root",
    Label = "custom-label",
    Description = "custom-description",
    Error = "custom-error",
    Content = "custom-content",
    Control = "custom-control",
};

foreach (var type in new[] { "text", "checkbox", "radio" })
{
    var helper = new StellarAdmin.TagHelpers.InputTagHelper(generator, icons)
    {
        ViewContext = viewContext,
        InputTypeName = type,
        ClassNames = inputClasses,
        Label = "Name",
        Description = "Help",
        Error = "Invalid",
    };
    var html = await Render(helper);
    Expect(html, "custom-root", "div", "sa-field");
    Expect(html, "custom-label", "label", "sa-field-label");
    Expect(html, "custom-description", "p", "sa-field-description");
    Expect(html, "custom-error", "div", "sa-field-error");
    Expect(
        html,
        "custom-control",
        type == "text" ? "input" : "span",
        type == "text" ? "sa-input" : "sa-input-control-wrapper"
    );
    Expect(
        html,
        "existing-class",
        "input",
        type == "text" ? "sa-input"
            : type == "radio" ? "sa-radiobutton"
            : "sa-checkbox"
    );

    if (type != "text")
    {
        Expect(html, "custom-content", "div", "sa-field-content");
        Expect(html, "custom-control", "span", "sa-input-control-wrapper");
    }

    helper.ShouldRenderField = false;
    html = await Render(helper);
    Require(
        !html.Contains("custom-root") && !html.Contains("custom-label"),
        "Suppressed field parts leaked onto the control."
    );
    Expect(
        html,
        "custom-control",
        type == "text" ? "input" : "span",
        type == "text" ? "sa-input" : "sa-input-control-wrapper"
    );
}

var select = new StellarAdmin.TagHelpers.SelectTagHelper(generator, icons)
{
    ViewContext = viewContext,
    ClassNames = new SelectClassNames { Control = "select-control" },
};
var selectHtml = await Render(select);
Expect(selectHtml, "select-control", "div", "sa-native-select-wrapper");
Expect(selectHtml, "existing-class", "div", "sa-native-select-wrapper");

var textareaHtml = await Render(
    new StellarAdmin.TagHelpers.TextareaTagHelper(generator)
    {
        ViewContext = viewContext,
        ClassNames = new TextareaClassNames { Control = "textarea-input" },
    }
);
Expect(textareaHtml, "textarea-input", "textarea", "sa-textarea");

var switchHtml = await Render(
    new SwitchTagHelper(generator)
    {
        ViewContext = viewContext,
        ClassNames = new SwitchClassNames { Control = "switch-control" },
    }
);
Expect(switchHtml, "switch-control", "span", "sa-switch-wrapper");

var sliderHtml = await Render(
    new SliderTagHelper(generator)
    {
        ViewContext = viewContext,
        ClassNames = new SliderClassNames
        {
            Control = "slider-control",
            Track = "slider-track",
            Range = "slider-range",
            Thumb = "slider-thumb",
        },
    }
);
Expect(sliderHtml, "slider-control", "sel-slider", "sa-slider");
Expect(sliderHtml, "slider-track", "span", "sa-slider-track");
Expect(sliderHtml, "slider-range", "span", "sa-slider-range");
Expect(sliderHtml, "slider-thumb", "span", "sa-slider-thumb");

foreach (var composed in new[] { false, true })
{
    var otp = new InputOtpTagHelper(generator, icons)
    {
        ViewContext = viewContext,
        Groups = "2,2",
        ClassNames = new InputOtpClassNames
        {
            Control = "otp-control",
            Group = "otp-group",
            Slot = "otp-slot",
            Separator = "otp-separator",
        },
    };
    var otpHtml = await Render(
        otp,
        composed
            ? async context =>
            {
                var group = await RenderChild(new InputOtpGroupTagHelper(), context);
                var slot = await RenderChild(new InputOtpSlotTagHelper(), context);
                var separator = await RenderChild(new InputOtpSeparatorTagHelper(icons), context);

                return group + slot + separator;
            }
            : null
    );
    Expect(otpHtml, "otp-control", "sel-input-otp", "sa-input-otp");
    Expect(otpHtml, "otp-group", "div", "sa-input-otp-group", composed ? 1 : 2);
    Expect(otpHtml, "otp-slot", "div", "sa-input-otp-slot", composed ? 1 : 4);
    Expect(otpHtml, "otp-separator", "div", "sa-input-otp-separator");
    Require(
        otpHtml.Contains("data-caret-class=\"sa-input-otp-caret\""),
        "OTP caret classes missing."
    );
    Require(
        otpHtml.Contains("data-caret-line-class=\"sa-input-otp-caret-line\""),
        "OTP caret line classes missing."
    );
}

var toggleHtml = await Render(
    new ToggleGroupTagHelper(generator)
    {
        ViewContext = viewContext,
        ClassNames = new ToggleGroupClassNames { Control = "toggle-control", Item = "toggle-item" },
    },
    context => RenderChild(new ToggleGroupItemTagHelper { Value = "one", Selected = true }, context)
);
Expect(toggleHtml, "toggle-control", "div", "sa-toggle-group");
Expect(toggleHtml, "toggle-item", "label", "sa-toggle-group-item");
Require(toggleHtml.Contains("toggle-item existing-class"), "Explicit item classes were lost.");
Require(toggleHtml.Contains("checked"), "Toggle selection was lost.");

viewContext.ViewData.ModelState.SetModelValue("Name", "submitted", "submitted");
viewContext.ViewData.ModelState.AddModelError("Name", "Required name.");
viewContext.ClientValidationEnabled = true;

var boundHtml = await Render(
    new StellarAdmin.TagHelpers.InputTagHelper(generator, icons)
    {
        ViewContext = viewContext,
        For = new ModelExpression(
            "Name",
            metadata.GetModelExplorerForType(typeof(string), "original")
        ),
        ClassNames = inputClasses,
    }
);
Expect(boundHtml, "custom-control", "input", "sa-input");
Expect(boundHtml, "custom-error", "div", "sa-field-error");
Require(boundHtml.Contains("value=\"submitted\""), "ModelState value was lost.");
Require(boundHtml.Contains("field-validation-error"), "MVC validation classes were lost.");
Require(
    !boundHtml.Contains("ClassAttributeHtmlContent"),
    "An MVC class attribute was stringified incorrectly."
);

Console.WriteLine("Field class name rendering checks passed.");

async Task<string> Render(
    FieldInputBaseTagHelper helper,
    Func<TagHelperContext, Task<string>>? children = null
)
{
    var attributes = new TagHelperAttributeList();
    var context = new TagHelperContext(
        attributes,
        new Dictionary<object, object>(),
        Guid.NewGuid().ToString()
    );
    if (helper is StellarAdmin.TagHelpers.InputTagHelper { InputTypeName: not null } input)
    {
        attributes.Add(new TagHelperAttribute("type", input.InputTypeName));
    }

    helper.Init(context);
    var output = new TagHelperOutput(
        "sa-test",
        new TagHelperAttributeList { { "class", "existing-class" } },
        async (_, _) =>
            new DefaultTagHelperContent().SetHtmlContent(
                children == null ? "" : await children(context)
            )
    );
    await helper.ProcessAsync(context, output);

    return Serialize(output);
}

async Task<string> RenderChild(TagHelper helper, TagHelperContext parent)
{
    var context = new TagHelperContext(
        new TagHelperAttributeList(),
        new Dictionary<object, object>(parent.Items),
        Guid.NewGuid().ToString()
    );
    helper.Init(context);
    var output = new TagHelperOutput(
        "sa-test",
        new TagHelperAttributeList { { "class", "existing-class" } },
        (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
    );
    await helper.ProcessAsync(context, output);

    return Serialize(output);
}

static string Serialize(TagHelperOutput output)
{
    using var writer = new StringWriter();
    output.WriteTo(writer, HtmlEncoder.Default);

    return writer.ToString();
}

static void Expect(
    string html,
    string customClass,
    string tag,
    string structuralClass,
    int count = 1
)
{
    var matches = Regex
        .Matches(html, "<(?<tag>[a-z0-9-]+)\\b[^>]* class=\"(?<classes>[^\"]*)\"[^>]*>")
        .Where(match => match.Groups["classes"].Value.Split(' ').Contains(customClass))
        .ToList();
    Require(
        matches.Count == count,
        $"Expected {count} {customClass} targets, got {matches.Count}: {html}"
    );
    Require(
        matches.All(match =>
            match.Groups["tag"].Value == tag
            && match.Groups["classes"].Value.Split(' ').Contains(structuralClass)
        ),
        $"{customClass} must reach {tag}.{structuralClass}: {html}"
    );
}

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
