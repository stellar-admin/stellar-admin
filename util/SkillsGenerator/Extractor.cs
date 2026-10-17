using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SkillsGenerator;

/// <summary>
/// Parses the tag-helper C# sources into <see cref="ComponentInfo"/> models using Roslyn syntax
/// trees only (no compilation / semantic model). Deterministic: all iteration is explicitly ordered.
/// </summary>
internal static partial class Extractor
{
    /// <summary>
    /// Builds the global enum dictionary (enum name -> ordered members) by scanning EVERY .cs file
    /// under the tag-helpers root, so cross-folder enum references (e.g. a DropdownMenu attribute
    /// typed as <c>ButtonSize</c> or <c>PopoverSide</c>) resolve.
    /// </summary>
    public static Dictionary<string, EnumInfo> BuildEnumIndex(string tagHelpersRoot)
    {
        var result = new Dictionary<string, EnumInfo>(StringComparer.Ordinal);

        foreach (var file in EnumerateCsFiles(tagHelpersRoot))
        {
            var root = ParseFile(file);
            foreach (var enumDecl in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
            {
                if (!IsPublic(enumDecl.Modifiers))
                    continue;

                var members = enumDecl
                    .Members.Select(m => new EnumMember(m.Identifier.Text, DocComments.Summary(m)))
                    .ToList();

                result[enumDecl.Identifier.Text] = new EnumInfo(enumDecl.Identifier.Text, members);
            }
        }

        return result;
    }

    /// <summary>
    /// Discovers every component folder (a direct subfolder of <paramref name="tagHelpersRoot"/>
    /// containing at least one <c>[HtmlTargetElement("...")]</c> class) and extracts its tags.
    /// Returned sorted by folder name.
    /// </summary>
    public static List<ComponentInfo> ExtractComponents(
        string tagHelpersRoot,
        Dictionary<string, EnumInfo> enums,
        Func<string, IReadOnlyList<ExampleInfo>> exampleLookup,
        IEnumerable<string>? sourceRoots = null
    )
    {
        var components = new List<ComponentInfo>();
        var classIndex = new Dictionary<string, ClassDeclarationSyntax>(StringComparer.Ordinal);
        foreach (var sourceRoot in sourceRoots ?? [tagHelpersRoot])
        {
            foreach (var entry in BuildClassIndex(sourceRoot))
            {
                classIndex.TryAdd(entry.Key, entry.Value);
            }
        }

        var folders = Directory
            .GetDirectories(tagHelpersRoot)
            .OrderBy(Path.GetFileName, StringComparer.Ordinal);

        foreach (var folder in folders)
        {
            var folderName = Path.GetFileName(folder);
            var tags = ExtractTagsForFolder(folder, enums, classIndex);
            if (tags.Count == 0)
                continue; // No [HtmlTargetElement] -> not a component (e.g. Menu/).

            var ordered = OrderTags(tags, out var primary);
            var examples = exampleLookup(folderName);

            components.Add(new ComponentInfo(folderName, ordered, primary, examples));
        }

        return components;
    }

    private static List<TagInfo> ExtractTagsForFolder(
        string folder,
        Dictionary<string, EnumInfo> enums,
        Dictionary<string, ClassDeclarationSyntax> classIndex
    )
    {
        var tags = new List<TagInfo>();

        // Sort files by name so the "source order" of tags/classes is deterministic across runs.
        var files = Directory
            .GetFiles(folder, "*.cs", SearchOption.AllDirectories)
            .OrderBy(Path.GetFileName, StringComparer.Ordinal);

        foreach (var file in files)
        {
            var root = ParseFile(file);
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var tagNames = TargetElementNames(classDecl);
                if (tagNames.Count == 0)
                    continue;

                var summary = DocComments.Summary(classDecl);
                var attributes = ExtractAttributes(classDecl, enums, classIndex);

                foreach (var tagName in tagNames)
                    tags.Add(new TagInfo(tagName, summary, attributes));
            }
        }

        return tags;
    }

    /// <summary>The tag names from every <c>[HtmlTargetElement("...")]</c> with a string-literal arg.</summary>
    private static List<string> TargetElementNames(ClassDeclarationSyntax classDecl)
    {
        var names = new List<string>();

        foreach (var attribute in classDecl.AttributeLists.SelectMany(a => a.Attributes))
        {
            var name = attribute.Name.ToString();
            if (name is not ("HtmlTargetElement" or "HtmlTargetElementAttribute"))
                continue;

            var firstArg = attribute.ArgumentList?.Arguments.FirstOrDefault();
            if (
                firstArg?.Expression is LiteralExpressionSyntax literal
                && literal.IsKind(SyntaxKind.StringLiteralExpression)
            )
            {
                names.Add(literal.Token.ValueText);
            }
        }

        return names;
    }

    private static List<AttributeInfo> ExtractAttributes(
        ClassDeclarationSyntax classDecl,
        Dictionary<string, EnumInfo> enums,
        Dictionary<string, ClassDeclarationSyntax> classIndex
    )
    {
        var attributes = new List<AttributeInfo>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Walk the declaring class first, then each base class in turn, so an attribute
        // redeclared on a derived type wins over the inherited one. Bound properties on the
        // shared bases (asp-for/label/description/error on the field-input base, the asp-*
        // routing attributes on the anchor base) are real attributes of every derived tag and
        // would otherwise be missing from the table entirely.
        foreach (var (declaration, typeArguments) in WithBaseClasses(classDecl, classIndex))
        foreach (var prop in declaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (!IsPublic(prop.Modifiers))
                continue;
            if (prop.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;
            if (!HasSetter(prop))
                continue;
            if (HasAttribute(prop, "HtmlAttributeNotBound"))
                continue;

            var name = AttributeName(prop);
            if (!seen.Add(name))
                continue;

            var typeText = SubstituteTypeArguments(prop.Type.ToString(), typeArguments);

            attributes.Add(
                new AttributeInfo(
                    name,
                    typeText,
                    DocComments.Summary(prop),
                    DocComments.Default(prop)
                )
            );

            // A dictionary-bound property is written as a prefixed attribute in practice
            // (`asp-route-id="1"`), which is the form authors actually reach for, so surface it
            // alongside the whole-dictionary name.
            if (DictionaryPrefix(prop) is { } prefix && seen.Add(prefix + "*"))
            {
                attributes.Add(
                    new AttributeInfo(prefix + "*", typeText, DocComments.Summary(prop), "—")
                );
            }
        }

        return attributes;
    }

    /// <summary>
    /// The <c>DictionaryAttributePrefix</c> of a property's <c>[HtmlAttributeName]</c>, or null
    /// when it has none.
    /// </summary>
    private static string? DictionaryPrefix(PropertyDeclarationSyntax prop)
    {
        foreach (var attribute in prop.AttributeLists.SelectMany(a => a.Attributes))
        {
            if (
                attribute.Name.ToString()
                is not ("HtmlAttributeName" or "HtmlAttributeNameAttribute")
            )
                continue;

            var argument = attribute
                .ArgumentList?.Arguments.FirstOrDefault(a =>
                    a.NameEquals?.Name.Identifier.Text == "DictionaryAttributePrefix"
                )
                ?.Expression;

            switch (argument)
            {
                case LiteralExpressionSyntax literal
                    when literal.IsKind(SyntaxKind.StringLiteralExpression):
                    return literal.Token.ValueText;

                case IdentifierNameSyntax identifier
                    when ConstantValue(prop, identifier.Identifier.Text) is { } constant:
                    return constant;
            }
        }

        return null;
    }

    private static string AttributeName(PropertyDeclarationSyntax prop)
    {
        foreach (var attribute in prop.AttributeLists.SelectMany(a => a.Attributes))
        {
            if (
                attribute.Name.ToString()
                is not ("HtmlAttributeName" or "HtmlAttributeNameAttribute")
            )
                continue;

            var firstArg = attribute.ArgumentList?.Arguments.FirstOrDefault();
            switch (firstArg?.Expression)
            {
                case LiteralExpressionSyntax literal
                    when literal.IsKind(SyntaxKind.StringLiteralExpression):
                    return literal.Token.ValueText;

                // The name is often held in a private const on the declaring class
                // (`[HtmlAttributeName(ForAttributeName)]`); without resolving it, the property
                // name is kebab-cased instead and the table shows the wrong attribute.
                case IdentifierNameSyntax identifier
                    when ConstantValue(prop, identifier.Identifier.Text) is { } constant:
                    return constant;
            }
        }

        return ToKebabCase(prop.Identifier.Text);
    }

    /// <summary>
    /// Resolves a <c>const string</c> declared on the type containing <paramref name="prop"/>,
    /// or null when there is no such constant with a literal string initializer.
    /// </summary>
    private static string? ConstantValue(PropertyDeclarationSyntax prop, string name)
    {
        var declaringType = prop.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();

        var initializer = declaringType
            ?.Members.OfType<FieldDeclarationSyntax>()
            .Where(f => f.Modifiers.Any(SyntaxKind.ConstKeyword))
            .SelectMany(f => f.Declaration.Variables)
            .FirstOrDefault(v => v.Identifier.Text == name)
            ?.Initializer?.Value;

        return
            initializer is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression)
            ? literal.Token.ValueText
            : null;
    }

    private static bool HasSetter(PropertyDeclarationSyntax prop)
    {
        var accessors = prop.AccessorList?.Accessors;
        if (accessors is null)
            return false; // expression-bodied get-only property

        return accessors.Value.Any(a =>
            a.IsKind(SyntaxKind.SetAccessorDeclaration)
            || a.IsKind(SyntaxKind.InitAccessorDeclaration)
        );
    }

    private static bool HasAttribute(PropertyDeclarationSyntax prop, string attributeName) =>
        prop
            .AttributeLists.SelectMany(a => a.Attributes)
            .Any(a =>
                a.Name.ToString() == attributeName
                || a.Name.ToString() == attributeName + "Attribute"
            );

    private static bool IsPublic(SyntaxTokenList modifiers) =>
        modifiers.Any(SyntaxKind.PublicKeyword);

    /// <summary>
    /// Orders tags for output: the primary tag first, then the rest alphabetically. The primary is
    /// the shortest tag whose name is a prefix of every other tag; failing that, the first declared.
    /// </summary>
    private static List<TagInfo> OrderTags(List<TagInfo> tags, out TagInfo primary)
    {
        var chosen = ChoosePrimary(tags);
        primary = chosen;

        var rest = tags.Where(t => t.Name != chosen.Name)
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

        var ordered = new List<TagInfo>(tags.Count) { chosen };
        ordered.AddRange(rest);
        return ordered;
    }

    private static TagInfo ChoosePrimary(List<TagInfo> tags)
    {
        if (tags.Count == 1)
            return tags[0];

        // The root tag is the one that is a prefix of the most other tags (e.g. sa-item for the
        // Item family, sa-sidebar for Sidebar). Ties break toward the shortest, then alphabetical.
        // This is robust to outlier tags that don't share the family prefix.
        return tags.OrderByDescending(t =>
                tags.Count(o =>
                    o.Name != t.Name && o.Name.StartsWith(t.Name + "-", StringComparison.Ordinal)
                )
            )
            .ThenBy(t => t.Name.Length)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .First();
    }

    /// <summary>
    /// Indexes every public class declared under the tag-helpers root by simple name, so a tag
    /// helper's base classes can be resolved from the syntax tree alone (there is no semantic
    /// model). Later declarations of the same name are ignored, which is fine here: tag helper
    /// class names are unique across the library.
    /// </summary>
    private static Dictionary<string, ClassDeclarationSyntax> BuildClassIndex(string tagHelpersRoot)
    {
        var result = new Dictionary<string, ClassDeclarationSyntax>(StringComparer.Ordinal);

        foreach (var file in EnumerateCsFiles(tagHelpersRoot))
        {
            var root = ParseFile(file);
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                result.TryAdd(
                    ClassKey(
                        classDecl.Identifier.Text,
                        classDecl.TypeParameterList?.Parameters.Count ?? 0
                    ),
                    classDecl
                );
        }

        return result;
    }

    /// <summary>
    /// Keys a class by name and arity, so a generic base such as
    /// <c>FieldInputBaseTagHelper&lt;TClassNames&gt;</c> doesn't collide with its non-generic base.
    /// </summary>
    private static string ClassKey(string name, int arity) => arity == 0 ? name : $"{name}`{arity}";

    /// <summary>
    /// The class and its base classes, each with the type arguments that replace its type
    /// parameters, so an inherited <c>TClassNames? ClassNames</c> shows the derived tag's type.
    /// </summary>
    private static IEnumerable<(
        ClassDeclarationSyntax Declaration,
        IReadOnlyDictionary<string, string> TypeArguments
    )> WithBaseClasses(
        ClassDeclarationSyntax classDecl,
        Dictionary<string, ClassDeclarationSyntax> classIndex
    )
    {
        var visited = new HashSet<ClassDeclarationSyntax>();
        var current = classDecl;
        IReadOnlyDictionary<string, string> typeArguments = new Dictionary<string, string>();

        while (current is not null && visited.Add(current))
        {
            yield return (current, typeArguments);

            var baseType = current
                .BaseList?.Types.Select(t => t.Type)
                .OfType<SimpleNameSyntax>()
                .FirstOrDefault();
            if (baseType is null)
                break;

            var arguments = baseType is GenericNameSyntax generic
                ? generic
                    .TypeArgumentList.Arguments.Select(a =>
                        SubstituteTypeArguments(a.ToString(), typeArguments)
                    )
                    .ToList()
                : [];

            if (
                !classIndex.TryGetValue(
                    ClassKey(baseType.Identifier.Text, arguments.Count),
                    out var baseDecl
                )
            )
                break;

            typeArguments = (baseDecl.TypeParameterList?.Parameters ?? default)
                .Select((parameter, index) => (parameter.Identifier.Text, arguments[index]))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2, StringComparer.Ordinal);
            current = baseDecl;
        }
    }

    private static string SubstituteTypeArguments(
        string typeText,
        IReadOnlyDictionary<string, string> typeArguments
    ) =>
        typeArguments.Count == 0
            ? typeText
            : Regex.Replace(
                typeText,
                @"\b\w+\b",
                match =>
                    typeArguments.TryGetValue(match.Value, out var argument)
                        ? argument
                        : match.Value
            );

    private static IEnumerable<string> EnumerateCsFiles(string root) =>
        Directory
            .GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.Ordinal);

    private static CompilationUnitSyntax ParseFile(string path) =>
        (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex KebabRegex();

    public static string ToKebabCase(string value) =>
        KebabRegex().Replace(value, "-$1").ToLowerInvariant();
}
