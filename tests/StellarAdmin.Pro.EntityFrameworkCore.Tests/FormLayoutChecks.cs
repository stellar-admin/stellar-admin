using System.Text.RegularExpressions;
using IdentitySimplePlayground.Data;
using StellarAdmin.Pro.Resources.Builders;
using StellarAdmin.Pro.Resources.Options;

internal static class FormLayoutChecks
{
    public static void CheckActions(string html, string indexUrl, bool canDelete)
    {
        var cancel = Regex
            .Match(
                html,
                """<a\b[^>]*data-slot="form-cancel"[^>]*>.*?</a>""",
                RegexOptions.Singleline
            )
            .Value;
        Require(
            cancel.Contains($"href=\"{indexUrl}\"", StringComparison.OrdinalIgnoreCase),
            $"Cancel navigates to {indexUrl} without the entity id: {cancel}"
        );
        Require(
            cancel.Contains("sa-button-variant-outline"),
            "Cancel is an outlined navigation link"
        );
        var triggers = Regex.Matches(
            html,
            """<button\b[^>]*commandfor="delete-confirm-dialog"[^>]*>"""
        );
        Require(
            triggers.Count == (canDelete ? 1 : 0),
            "Delete appears once on edit and is absent on create"
        );
        if (canDelete)
        {
            Require(
                triggers[0].Value.Contains("sa-button-variant-ghost")
                    && triggers[0].Value.Contains("type=\"button\""),
                "Delete is a quiet non-submit trigger"
            );
            Require(
                html.IndexOf("sa-resource-form-delete", StringComparison.Ordinal)
                    < html.IndexOf("data-slot=\"form-cancel\"", StringComparison.Ordinal),
                "Delete precedes Cancel and Save"
            );
            Require(
                html.Contains("sa-button-variant-destructive"),
                "Delete confirmation retains destructive emphasis"
            );
        }
    }

    public static void CheckConfiguration()
    {
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<Product>(options);
        FormFieldsBuilder<Product>? root = null;
        FormFieldsBuilder<Product>? children = null;

        resource.Create(create =>
            create.Fields(fields =>
            {
                root = fields;
                fields.AddSection(
                    "Details",
                    section =>
                    {
                        section.Description = "Description";
                        section.Fields(fields =>
                        {
                            children = fields;
                            fields.Add(product => product.Sku);
                            fields.AddGroup(group =>
                                group.Fields(fields =>
                                {
                                    fields.AddRow(row =>
                                        row.Fields(fields =>
                                        {
                                            fields.Add(product => product.CreatedAt).ReadOnly();
                                        })
                                    );
                                })
                            );
                        });
                    }
                );
            })
        );

        Require(
            options
                .CreatePage.Fields.Select(field => field.FieldName)
                .SequenceEqual(["Name", "Sku", "CreatedAt"]),
            "Nested fields extend seeded defaults in order"
        );
        Require(options.CreatePage.Items.Count == 2, "Layout retains section structure");
        Require(options.CreatePage.Fields.Last().IsReadOnly, "Nested read-only setting preserved");

        children!.Clear();
        children
            .Add(product => product.Price)
            .Template("Currency")
            .Title("Retail price")
            .Editor(editor => editor.ClassNames.Root = "retail-price");

        Require(
            options
                .CreatePage.Fields.Select(field => field.FieldName)
                .SequenceEqual(["Name", "Price"]),
            "Nested Clear preserves the parent and siblings"
        );
        Require(
            options.CreatePage.Fields.Last().Template == "Currency"
                && options.CreatePage.Fields.Last().Title == "Retail price",
            "Nested field settings preserved"
        );

        Require(
            options.CreatePage.Fields.Last().Editor.ClassNames.Root == "retail-price",
            "Nested field retains editor options"
        );
        Require(
            options.CreatePage.Fields.First().Editor.ClassNames.Root is null,
            "Editor options are isolated per field"
        );

        var typed = children
            .Add(product => product.Condition)
            .Editor(editor => editor.ClassNames.Root = "condition-root")
            .Editor<RadioEditorOptions>(editor =>
                editor.ClassNames.Option.Root = "condition-choice"
            )
            .Editor(editor => editor.ClassNames.Label = "condition-label")
            .Editor<RadioEditorOptions>(editor => editor.ClassNames.Control = "condition-options");
        var radio = (RadioEditorOptions)options.CreatePage.Fields.Last().Editor;
        Require(
            radio.ClassNames.Root == "condition-root"
                && radio.ClassNames.Label == "condition-label"
                && radio.ClassNames.Control == "condition-options"
                && radio.ClassNames.Option.Root == "condition-choice",
            "Common and typed configuration compose without losing prior values"
        );

        try
        {
            typed.Editor<OtherEditorOptions>(_ => { });
            throw new Exception("Expected incompatible configuration to fail.");
        }
        catch (InvalidOperationException exception)
        {
            Require(
                exception.Message.Contains("Condition")
                    && exception.Message.Contains("OtherEditorOptions"),
                "Configuration error identifies the field and conflicting options"
            );
        }

        root!.Clear();
        Require(
            options.CreatePage.Items.Count == 0 && options.CreatePage.Fields.Count == 0,
            "Root Clear removes fields and containers without fallback"
        );

        Console.WriteLine("All form layout configuration checks passed.");
    }

    public static void CheckCondition(string html, string selected)
    {
        Require(
            Regex.IsMatch(
                html,
                """<div[^>]*class="[^"]*sa-field-group[^"]*grid-cols-1[^"]*"[^>]*>"""
            ),
            "Choice-card editor applies the configured class to its choices container"
        );

        var radios = Regex
            .Matches(html, """<input[^>]*name="Entity.Condition"[^>]*>""")
            .Select(match => match.Value)
            .ToArray();
        Require(
            radios.Length == 3 && radios.All(radio => radio.Contains("""type="radio" """.Trim())),
            "Condition automatically uses the radio editor"
        );
        Require(
            radios.Count(radio => radio.Contains("checked")) == 1
                && radios
                    .Single(radio => radio.Contains("checked"))
                    .Contains($"value=\"{selected}\""),
            "Condition selection renders correctly"
        );

        Require(
            Regex
                .Matches(
                    html,
                    """<label[^>]*for="Entity_Condition_[^"]+"[^>]*>\s*<div[^>]*data-orientation="horizontal" """.Trim(),
                    RegexOptions.Singleline
                )
                .Count == 3,
            "Each condition is rendered inside a clickable choice card"
        );
        Require(
            html.Contains("An unused item in its original condition.")
                && html.Contains("A previously owned item restored and tested for resale.")
                && html.Contains("A previously owned item that may show signs of wear."),
            "Enum member descriptions render in choice cards"
        );

        foreach (var value in new[] { "New", "Refurbished", "Used" })
        {
            Require(
                html.Contains($"for=\"Entity_Condition_{value}\""),
                "Radio has a matching label"
            );
        }

        Require(
            Regex.Matches(html, "Select the condition of the item being sold.").Count == 1,
            "Radio group help text renders once"
        );
    }

    public static void CheckProductMarkup(string html)
    {
        var headings = Regex
            .Matches(
                html,
                """<h2[^>]*data-slot="form-section-title"[^>]*>(.*?)</h2>""",
                RegexOptions.Singleline
            )
            .Select(match => match.Groups[1].Value.Trim());
        Require(
            headings.SequenceEqual(["Details", "Pricing", "Inventory", "Shipping", "Publishing"]),
            "Sections render in order through headings"
        );
        Require(
            Regex.Matches(html, @"<fieldset\b").Count == 1,
            "The radio editor retains its fieldset"
        );
        Require(
            html.Contains("How this product appears in the catalog."),
            "Section description renders"
        );
        Require(Regex.Matches(html, "data-slot=\"form-row\"").Count == 3, "Responsive rows render");
        Require(
            Regex.Matches(html, "data-slot=\"form-row-content\"").Count == 3,
            "Rows use the shared grid component"
        );
        Require(
            Regex.Matches(html, "data-layout=\"split\"").Count == 5,
            "Sections default to Split"
        );
        Require(!html.Contains("--sa-form-row-columns"), "Rows do not emit legacy column counts");
        Require(!html.Contains("<sa-"), "All form tag helpers execute");

        foreach (
            var name in new[]
            {
                "Name",
                "Sku",
                "Description",
                "Price",
                "CompareAtPrice",
                "CostPrice",
                "StockQuantity",
                "WeightKg",
                "WidthCm",
                "HeightCm",
                "DepthCm",
                "PublishedAt",
                "CreatedAt",
            }
        )
        {
            Require(
                Regex
                    .Matches(html, $"""<(?:input|textarea)\b[^>]*name="Entity.{name}"[^>]*>""")
                    .Count == 1,
                $"{name} renders once with its original binding prefix"
            );
        }
    }

    private static void Require(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException(description);
        }
    }

    private sealed class OtherEditorOptions : EditorOptions { }

    private sealed class TestResourceOptions()
        : ResourceOptions<Product>(
            new IndexPageDefaults("Products", "Create", "Empty", "", "database", [], null),
            new FormPageDefaults("Create", "Create", ["Name"]),
            new FormPageDefaults("Edit", "Save", []),
            new DeleteDefaults<Product>("Delete", "Delete?", "Delete", "Cancel", _ => null)
        );
}
