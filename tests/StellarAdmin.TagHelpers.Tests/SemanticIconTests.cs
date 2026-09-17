using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin;
using StellarAdmin.Icons;
using StellarAdmin.TagHelpers;

internal static class SemanticIconTests
{
    public static async Task Run()
    {
        var options = new IconOptions();
        foreach (var role in Enum.GetValues<SemanticIconRole>())
        {
            var name = options.GetSemanticIconName(role);
            Require(
                name is not null && options.TryGetIcon(name, out _),
                $"Missing default for {role}."
            );
        }

        var defaultHtml = await Render(new PaginationEllipsisTagHelper(Options.Create(options)));
        Require(defaultHtml.Contains("<circle"), "Default pagination must render Lucide ellipsis.");

        options.AddIconPack<ReplacementPack>();
        Require(
            options.GetSemanticIconName(SemanticIconRole.PaginationEllipsis) == "TEST-DOTS",
            "Incoming mappings must win case-insensitively."
        );
        Require(
            options.GetSemanticIconName(SemanticIconRole.Close) == "x",
            "Partial packs must preserve other mappings."
        );
        options.AddIconPack<LegacyPack>();
        Require(
            options.GetSemanticIconName(SemanticIconRole.PaginationEllipsis) == "TEST-DOTS",
            "Legacy packs must preserve mappings."
        );

        options.ClearIcons();
        Require(
            Enum.GetValues<SemanticIconRole>()
                .All(role => options.GetSemanticIconName(role) is null),
            "Clear must remove all mappings."
        );
        options.AddIconPack<ReplacementPack>();
        Require(
            !options.TryGetIcon("ellipsis", out _),
            "Replacing a pack must not restore Lucide."
        );
        var rendered = await Render(new PaginationEllipsisTagHelper(Options.Create(options)));
        Require(
            rendered.Contains("data-test-icon=\"replacement\"") && rendered.Contains("More pages"),
            "Pagination must use the replacement pack and preserve its label."
        );
        var custom = await Render(
            new PaginationEllipsisTagHelper(Options.Create(options)),
            "<b>Custom pages</b>"
        );
        Require(
            custom.Contains("Custom pages") && !custom.Contains("<svg"),
            "Child content must still override the default icon."
        );

        options.MapSemanticIcon(SemanticIconRole.AccordionIndicator, "test-dots");
        var accordion = await Render(
            new AccordionItemTitleTagHelper(Options.Create(options)),
            "Accordion title"
        );
        Require(
            accordion.Contains("data-test-icon=\"replacement\"")
                && accordion.Contains("sa-accordion-trigger-icon")
                && accordion.Contains("Accordion title"),
            "Accordion must use the semantic icon without Lucide and preserve its title and wrapper."
        );

        options.MapSemanticIcon(SemanticIconRole.BreadcrumbEllipsis, "test-dots");
        options.AddIconPack<OverridePack>();
        var overridden = await Render(new BreadcrumbEllipsisTagHelper(Options.Create(options)));
        Require(
            overridden.Contains("data-test-icon=\"override\""),
            "Named overrides must affect semantic uses."
        );
        Require(
            new IconOptions().GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis)
                == "ellipsis",
            "Mappings must be isolated between options instances."
        );

        Require(options.RemoveIcon("TeSt-DoTs"), "Removal must ignore casing.");
        Require(
            options.GetSemanticIconName(SemanticIconRole.PaginationEllipsis) is null
                && options.GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis) is null,
            "Removal must clear every mapping targeting an icon."
        );
        var missing = await Render(new PaginationEllipsisTagHelper(Options.Create(options)));
        Require(missing.Contains("M12 9v4"), "Missing roles must render the existing fallback.");

        Reject(() => options.MapSemanticIcon(SemanticIconRole.Close, "unregistered"));
        Reject(() => options.AddIconPack<InvalidPack>());
        Require(
            options.GetIconNames().Length == 0,
            "An invalid mapping must not partially register a pack."
        );

        options.AddIcon("test-dots", ReplacementPack.Icon);
        Reject(() => options.AddIconPack<MappingOnlyPack>());
        Require(
            options.GetSemanticIconName(SemanticIconRole.Close) is null
                && options.TryGetIcon("test-dots", out var retainedIcon)
                && retainedIcon == ReplacementPack.Icon,
            "A pack must supply its mapping targets even when they are already registered."
        );
        options.MapSemanticIcon(SemanticIconRole.Close, "test-dots");
        Require(
            options.GetSemanticIconName(SemanticIconRole.Close) == "test-dots",
            "Application overrides may target an already registered icon."
        );
        options.AddIconPack<LucideIconPack>();
        Require(
            options.GetSemanticIconName(SemanticIconRole.Close) == "x",
            "Later packs must replace role overrides."
        );

        var retained = new IconOptions();
        var retainedNames = retained.GetIconNames();
        var retainedClose = retained.GetSemanticIconName(SemanticIconRole.Close);
        Require(retained.TryGetIcon("x", out var retainedCloseIcon), "Default close icon missing.");
        Reject(() => retained.AddIconPack<PartiallyValidPack>());
        Require(
            retained.GetIconNames().SequenceEqual(retainedNames)
                && retained.GetSemanticIconName(SemanticIconRole.Close) == retainedClose
                && retained.TryGetIcon("x", out var unchangedCloseIcon)
                && unchangedCloseIcon == retainedCloseIcon,
            "A later invalid mapping must preserve existing icons and earlier role mappings."
        );
        retained.AddIconPack<UnreadableMappingsPack>(pack => pack.ImportSemanticMappings = false);
        Require(
            retained.TryGetIcon("test-dots", out _),
            "Disabled mapping imports must not read the pack's mappings."
        );

        var services = new ServiceCollection();
        services.AddStellarAdmin().AddIconPack<ReplacementPack>(pack => pack.Prefix = "my:");
        using var provider = services.BuildServiceProvider();
        var prefixed = provider.GetRequiredService<IOptions<IconOptions>>().Value;
        Require(
            prefixed.TryGetIcon("MY:test-dots", out var prefixedIcon)
                && prefixedIcon == ReplacementPack.Icon
                && !prefixed.TryGetIcon("test-dots", out _)
                && prefixed.GetIconNames().Contains("my:test-dots"),
            "Prefixes must register only qualified names and preserve case-insensitive lookup."
        );
        Require(
            prefixed.GetSemanticIconName(SemanticIconRole.PaginationEllipsis) == "my:TEST-DOTS",
            "Imported mapping targets must receive the same literal prefix."
        );
        var prefixedHtml = await Render(new PaginationEllipsisTagHelper(Options.Create(prefixed)));
        Require(
            prefixedHtml.Contains("data-test-icon=\"replacement\""),
            "Prefixed semantic icons must render."
        );

        prefixed.AddIconPack<OverridePack>(pack => pack.Prefix = "other-");
        Require(
            prefixed.TryGetIcon("my:test-dots", out var original)
                && original == ReplacementPack.Icon
                && prefixed.TryGetIcon("other-test-dots", out _),
            "Different literal prefixes must let same-named icons coexist."
        );
        prefixed.MapSemanticIcon(SemanticIconRole.Close, "my:test-dots");
        Require(prefixed.RemoveIcon("MY:TEST-DOTS"), "Prefixed removal must ignore casing.");
        Require(
            prefixed.GetSemanticIconName(SemanticIconRole.Close) is null
                && prefixed.GetSemanticIconName(SemanticIconRole.PaginationEllipsis) is null,
            "Removing a prefixed icon must clear its mappings."
        );

        var additive = new IconOptions();
        additive.AddIconPack<ReplacementPack>(pack =>
        {
            pack.Prefix = "extra:";
            pack.ImportSemanticMappings = false;
        });
        Require(
            additive.GetSemanticIconName(SemanticIconRole.PaginationEllipsis) == "ellipsis"
                && additive.TryGetIcon("extra:test-dots", out _),
            "Disabling mapping imports must preserve existing mappings while adding icons."
        );
        additive.AddIconPack<ReplacementPack>(pack =>
        {
            pack.Prefix = "";
            pack.ImportSemanticMappings = false;
        });
        Require(
            additive.TryGetIcon("test-dots", out _),
            "An empty prefix must leave names unchanged."
        );
        additive.AddIconPack<InvalidPack>(pack => pack.ImportSemanticMappings = false);
        Require(
            additive.TryGetIcon("new-icon", out _),
            "Disabled mappings must not be read or validated."
        );
        Reject(() => additive.AddIconPack<InvalidPack>(pack => pack.Prefix = "bad:"));
        Require(
            !additive.TryGetIcon("bad:new-icon", out _),
            "Invalid prefixed mappings must not partially register icons."
        );

        Console.WriteLine("Semantic icon registration and rendering checks passed.");
    }

    private static void Reject(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new InvalidOperationException("An unregistered mapping target must be rejected.");
    }

    private static async Task<string> Render(TagHelper helper, string childContent = "")
    {
        var context = new TagHelperContext([], new Dictionary<object, object>(), "semantic-test");
        var output = new TagHelperOutput(
            "sa-test",
            [],
            (_, _) =>
                Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent().SetHtmlContent(childContent)
                )
        );
        helper.Init(context);
        await helper.ProcessAsync(context, output);
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class InvalidPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition> { ["new-icon"] = ReplacementPack.Icon };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string> { [SemanticIconRole.Close] = "absent" };
    }

    private sealed class LegacyPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>();

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
        {
            return new Dictionary<SemanticIconRole, string>();
        }
    }

    private sealed class MappingOnlyPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>();

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string> { [SemanticIconRole.Close] = "test-dots" };
    }

    private sealed class OverridePack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>
            {
                ["test-dots"] = new(
                    new Dictionary<string, string> { ["data-test-icon"] = "override" },
                    []
                ),
            };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
        {
            return new Dictionary<SemanticIconRole, string>();
        }
    }

    private sealed class PartiallyValidPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>
            {
                ["x"] = ReplacementPack.Icon,
                ["new-icon"] = ReplacementPack.Icon,
            };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string>
            {
                [SemanticIconRole.Close] = "new-icon",
                [SemanticIconRole.PaginationEllipsis] = "absent",
            };
    }

    private sealed class ReplacementPack : IIconPack
    {
        public static IconDefinition Icon { get; } =
            new(new Dictionary<string, string> { ["data-test-icon"] = "replacement" }, []);

        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition> { ["test-dots"] = Icon };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string>
            {
                [SemanticIconRole.PaginationEllipsis] = "TEST-DOTS",
            };
    }

    private sealed class UnreadableMappingsPack : IIconPack
    {
        public IDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition> { ["test-dots"] = ReplacementPack.Icon };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            throw new InvalidOperationException("Mappings must not be read.");
    }
}
