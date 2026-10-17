using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the base class for StellarAdmin tag helpers.
/// </summary>
/// <remarks>
///     A tag helper can find its StellarAdmin ancestors, share context with its descendants and
///     receive content for named slots.
/// </remarks>
public abstract class StellarAdminTagHelperBase : TagHelper
{
    private const string ParentTagHelperKey = "stellar-admin-parent-tag-helper";

    private readonly Dictionary<string, TagHelperContent> _namedSlots =
        new Dictionary<string, TagHelperContent>();

    /// <summary>
    ///     Gets the closest StellarAdmin tag helper that contains this one.
    /// </summary>
    /// <value>
    ///     The containing tag helper, or <see langword="null" /> if no StellarAdmin tag helper
    ///     contains this one.
    /// </value>
    [HtmlAttributeNotBound]
    protected internal StellarAdminTagHelperBase? ParentTagHelper { get; private set; }

    /// <summary>
    ///     Initializes the tag helper.
    /// </summary>
    /// <remarks>
    ///     Override <see cref="OnInit" /> to run code when the tag helper is initialized.
    /// </remarks>
    /// <param name="context">The context of the executing tag helper.</param>
    public sealed override void Init(TagHelperContext context)
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

        OnInit(context);
    }

    internal bool TryAddNamedSlot(string name, TagHelperContent childContent)
    {
        return _namedSlots.TryAdd(name, childContent);
    }

    /// <summary>
    ///     Gets the content that a child <c>&lt;sa-slot-content&gt;</c> element supplied for a
    ///     named slot.
    /// </summary>
    /// <param name="name">The name of the slot.</param>
    /// <param name="content">
    ///     When this method returns, contains the content of the slot, or <see langword="null" />
    ///     if the slot has no content. This parameter is treated as uninitialized.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the slot has content; otherwise, <see langword="false" />.
    /// </returns>
    protected internal bool TryGetNamedSlot(
        string name,
        [NotNullWhen(true)] out TagHelperContent? content
    )
    {
        return _namedSlots.TryGetValue(name, out content);
    }

    /// <summary>
    ///     Joins CSS class names into a single <c>class</c> attribute value.
    /// </summary>
    /// <remarks>
    ///     Entries that are <see langword="null" />, empty or white space are skipped.
    /// </remarks>
    /// <param name="classes">The class names to join.</param>
    /// <returns>The class names, separated by spaces.</returns>
    protected internal static string JoinCssClasses(params string?[] classes)
    {
        return string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    /// <summary>
    ///     Runs when the tag helper is initialized, before its child content runs.
    /// </summary>
    /// <remarks>
    ///     Override this method to share context with descendants by calling
    ///     <see cref="SetContext{T}" />.
    /// </remarks>
    /// <param name="context">The context of the executing tag helper.</param>
    protected virtual void OnInit(TagHelperContext context) { }

    /// <summary>
    ///     Shares a context object with descendant tag helpers.
    /// </summary>
    /// <remarks>
    ///     Descendants read the object with <see cref="GetContext{T}" />. Call this method before
    ///     the child content runs, such as in <see cref="OnInit" />.
    /// </remarks>
    /// <typeparam name="T">The type that descendants use to read the object.</typeparam>
    /// <param name="context">The context of the executing tag helper.</param>
    /// <param name="value">The object to share.</param>
    protected void SetContext<T>(TagHelperContext context, T value)
        where T : class
    {
        context.Items[typeof(T)] = value;
    }

    /// <summary>
    ///     Gets a context object that an ancestor tag helper shared with
    ///     <see cref="SetContext{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="context">The context of the executing tag helper.</param>
    /// <returns>
    ///     The shared object, or <see langword="null" /> if no ancestor shared one.
    /// </returns>
    protected T? GetContext<T>(TagHelperContext context)
        where T : class
    {
        return context.Items.TryGetValue(typeof(T), out var value) ? value as T : null;
    }

    // The grid this tag helper places itself in: the sa-grid or sa-details it is a direct child
    // of. The grid's context reaches every descendant, so one nested deeper, such as a field in a
    // section in a grid item, is not placed in it.
    private protected GridContext? GetEnclosingGrid(TagHelperContext context)
    {
        var grid = GetContext<GridContext>(context);

        return grid is not null && ReferenceEquals(grid.Owner, ParentTagHelper) ? grid : null;
    }

    /// <summary>
    ///     Returns an identifier that is unique to this element, for use in element ids.
    /// </summary>
    /// <remarks>
    ///     The identifier stays unique when a container renders the same markup more than once and
    ///     shares a <see cref="UniqueIdDiscriminator" />.
    /// </remarks>
    /// <param name="context">The context of the executing tag helper.</param>
    /// <returns>The unique identifier.</returns>
    protected string GetUniqueId(TagHelperContext context)
    {
        return GetContext<UniqueIdDiscriminator>(context)?.Value is { Length: > 0 } discriminator
            ? $"{context.UniqueId}-{discriminator}"
            : context.UniqueId;
    }

    /// <summary>
    ///     Returns the closest containing tag helper of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the tag helper to find.</typeparam>
    /// <returns>
    ///     The closest containing tag helper of type <typeparamref name="T" />, or
    ///     <see langword="null" /> if none contains this one.
    /// </returns>
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
}
