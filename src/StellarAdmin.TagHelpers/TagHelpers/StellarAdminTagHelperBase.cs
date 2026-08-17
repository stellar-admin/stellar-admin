using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Base class for all StellarAdmin tag helpers. Provides class-string composition,
///     named-slot support, and tracks the parent tag-helper stack so children can
///     locate their parents.
/// </summary>
public class StellarAdminTagHelperBase : TagHelper
{
    private const string ParentTagHelperKey = "stellar-admin-parent-tag-helper";

    private readonly Dictionary<string, TagHelperContent> _namedSlots =
        new Dictionary<string, TagHelperContent>();

    /// <summary>
    ///     The immediate ancestor StellarAdmin tag helper, or <c>null</c> when this is a root.
    /// </summary>
    [HtmlAttributeNotBound]
    protected internal StellarAdminTagHelperBase? ParentTagHelper { get; private set; }

    public override void Init(TagHelperContext context)
    {
        // Razor gives every tag helper scope its own copy-on-write view of Items, so a value
        // set here is visible to descendants only: siblings and ancestors keep seeing their
        // own parent. That per-scope copy is what makes this a stack without any popping.
        ParentTagHelper =
            context.Items.TryGetValue(ParentTagHelperKey, out var parent)
            && parent is StellarAdminTagHelperBase parentTagHelper
                ? parentTagHelper
                : null;

        // Slots are transparent: their content belongs to the enclosing host.
        if (this is not SlotContentTagHelper)
        {
            context.Items[ParentTagHelperKey] = this;
        }
    }

    /// <summary>
    ///     Stores content under a slot name. Returns <c>false</c> when the slot has already
    ///     been filled.
    /// </summary>
    public bool TryAddNamedSlot(string name, TagHelperContent childContent)
    {
        return _namedSlots.TryAdd(name, childContent);
    }

    /// <summary>
    ///     Gets the content a child <c>&lt;sa-slot-content&gt;</c> assigned to a named slot.
    /// </summary>
    public bool TryGetNamedSlot(string name, [NotNullWhen(true)] out TagHelperContent? content)
    {
        return _namedSlots.TryGetValue(name, out content);
    }

    /// <summary>
    ///     Joins class strings into a single <c>class</c> attribute value, skipping null and
    ///     empty entries. Component class names are emitted verbatim; conflict resolution is
    ///     the stylesheet's job (author utilities out-rank component rules by cascade layer).
    /// </summary>
    protected internal static string JoinCssClasses(params string?[] classes)
    {
        return string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    /// <summary>
    ///     Publishes a context object that descendant tag helpers can read with
    ///     <see cref="GetContext{T}" />. The value is keyed by its type and stored in
    ///     <see cref="TagHelperContext.Items" />, so it must be set before the parent renders its
    ///     children (i.e. before <c>GetChildContentAsync</c>).
    /// </summary>
    protected void SetContext<T>(TagHelperContext context, T value)
        where T : class
    {
        context.Items[typeof(T)] = value;
    }

    /// <summary>
    ///     Reads a context object published by an ancestor tag helper with
    ///     <see cref="SetContext{T}" />, or <c>null</c> if no ancestor published one.
    /// </summary>
    protected T? GetContext<T>(TagHelperContext context)
        where T : class
    {
        return context.Items.TryGetValue(typeof(T), out var value) ? value as T : null;
    }

    /// <summary>
    ///     Returns an identifier unique to this element, for minting DOM ids. Equal to
    ///     <see cref="TagHelperContext.UniqueId" /> (a per-source-tag compile-time literal)
    ///     unless an ancestor repeating container published a <see cref="UniqueIdDiscriminator" />,
    ///     in which case the discriminator is appended so ids stay unique when the same markup
    ///     is executed multiple times.
    /// </summary>
    protected string GetUniqueId(TagHelperContext context)
    {
        return GetContext<UniqueIdDiscriminator>(context)?.Value is { Length: > 0 } discriminator
            ? $"{context.UniqueId}-{discriminator}"
            : context.UniqueId;
    }

    protected T? GetParentTagHelper<T>()
        where T : StellarAdminTagHelperBase
    {
        var currentParentTagHelper = ParentTagHelper;
        while (currentParentTagHelper is not null)
        {
            if (currentParentTagHelper is T asT)
            {
                return asT;
            }

            currentParentTagHelper = currentParentTagHelper.ParentTagHelper;
        }

        return null;
    }

    protected string? GetUserSpecifiedClass(TagHelperOutput output)
    {
        if (
            output.Attributes.ContainsName("class")
            && output.Attributes["class"].Value?.ToString() is { } userSpecifiedClass
        )
        {
            return userSpecifiedClass;
        }

        return null;
    }
}
