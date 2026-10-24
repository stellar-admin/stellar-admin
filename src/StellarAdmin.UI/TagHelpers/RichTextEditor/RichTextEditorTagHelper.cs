using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an editor for formatted text, such as headings, lists, links, tables and images,
///     whose value is HTML.
/// </summary>
/// <remarks>
///     <para>
///         The editor posts its HTML in a <c>&lt;textarea&gt;</c>, which shows instead of the
///         editor until the editor's script has loaded. The editor renders the module script that
///         loads it, so the script loads only on pages with an editor, including editors in htmx
///         responses. Requires <c>stellar-admin.js</c> for the toolbar's menus and tooltips.
///     </para>
///     <para>
///         The posted HTML comes from the browser. Sanitize it on the server before you store or
///         display it.
///     </para>
///     <para>
///         With <see cref="UploadUrl" />, images can be added. Each image is posted to it as the
///         multipart field <c>file</c>, with the form's antiforgery token. The endpoint answers
///         with JSON: <c>{ "url": "..." }</c> on success, or <c>{ "error": "..." }</c> with an
///         error status. The editor shows the image from that URL.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-rich-text-editor", TagStructure = TagStructure.WithoutEndTag)]
public sealed class RichTextEditorTagHelper : FieldInputBaseTagHelper<RichTextEditorClassNames>
{
    private const string DefaultAccept = "image/jpeg,image/png,image/webp,image/gif,image/avif";
    private const long DefaultMaxFileSize = 5 * 1024 * 1024;
    private const int DefaultRows = 8;

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextEditorTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that renders the bound textarea and the field's label and messages.
    /// </param>
    /// <param name="iconOptions">The icon options that supply the toolbar's icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public RichTextEditorTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the image types that can be added.
    /// </summary>
    /// <value>
    ///     A comma-separated list of media types. The default is JPEG, PNG, WebP, GIF and AVIF.
    /// </value>
    [HtmlAttributeName("accept")]
    public string? Accept { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the editor is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the editor is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A disabled editor can't be focused, and its value isn't posted.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the largest image that can be added.
    /// </summary>
    /// <value>
    ///     The size in bytes. The default is <c>5242880</c> (5 MB).
    /// </value>
    /// <remarks>
    ///     The browser checks the size before uploading. The upload endpoint must check it again.
    /// </remarks>
    [HtmlAttributeName("max-file-size")]
    public long? MaxFileSize { get; set; }

    /// <summary>
    ///     Gets or sets the hint text displayed while the editor is empty.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the text can't be changed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the text can't be changed; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A read-only editor has no toolbar, but can still be focused, and its value is posted.
    /// </remarks>
    [HtmlAttributeName("readonly")]
    public bool? ReadOnly { get; set; }

    /// <summary>
    ///     Gets or sets the number of lines of text the editor has room for before it grows.
    /// </summary>
    /// <value>
    ///     The number of lines. The default is <c>8</c>.
    /// </value>
    /// <remarks>
    ///     The editor grows with its content up to a maximum height, then scrolls.
    /// </remarks>
    [HtmlAttributeName("rows")]
    public int? Rows { get; set; }

    /// <summary>
    ///     Gets or sets the URL that an added image is posted to.
    /// </summary>
    /// <value>
    ///     The URL, or <see langword="null" /> to leave out adding images. The default is
    ///     <see langword="null" />.
    /// </value>
    [HtmlAttributeName("upload-url")]
    public string? UploadUrl { get; set; }

    /// <summary>
    ///     Gets or sets the HTML to edit.
    /// </summary>
    /// <remarks>
    ///     Used when the editor isn't bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Rows" /> or <see cref="MaxFileSize" /> isn't positive.
    /// </exception>
    protected override Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var rows = Rows ?? DefaultRows;
        if (rows <= 0)
        {
            throw new InvalidOperationException("A rich text editor's rows must be positive.");
        }

        var maxFileSize = MaxFileSize ?? DefaultMaxFileSize;
        if (maxFileSize <= 0)
        {
            throw new InvalidOperationException(
                "A rich text editor's max-file-size must be positive."
            );
        }

        var disabled = Disabled ?? false;
        var readOnly = !disabled && (ReadOnly ?? false);
        var editable = !disabled && !readOnly;
        var uploads = editable && !string.IsNullOrEmpty(UploadUrl);
        var id = $"--sa-rich-text-editor-{GetUniqueId(context)}";

        output.Attributes.RemoveAll("name");
        output.TagName = "sel-rich-text-editor";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "rich-text-editor");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-rich-text-editor",
                ClassNames?.Control,
                output.GetUserSuppliedClass()
            )
        );
        if (disabled)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
        }

        if (readOnly)
        {
            output.Attributes.SetAttribute("data-readonly", "true");
        }

        if (uploads)
        {
            output.Attributes.SetAttribute("data-upload-url", UploadUrl);
            output.Attributes.SetAttribute(
                "data-accept",
                string.IsNullOrWhiteSpace(Accept) ? DefaultAccept : Accept
            );
            output.Attributes.SetAttribute(
                "data-max-file-size",
                maxFileSize.ToString(CultureInfo.InvariantCulture)
            );
        }

        // The label names the editing area and the textarea, which aren't both labelable
        if (WillRenderFieldLabel())
        {
            LabelId = $"sa-{GetUniqueId(context)}-label";
        }

        var editor = new TagHelperAttributeList
        {
            { "data-slot", "rich-text-editor-content" },
            {
                "class",
                JoinCssClasses("sa-rich-text-editor-content sa-rich-text", ClassNames?.Editor)
                    ?? string.Empty
            },
            { "role", "textbox" },
            { "aria-multiline", "true" },
            { "style", $"--_rows: {rows.ToString(CultureInfo.InvariantCulture)}" },
        };
        if (LabelId != null)
        {
            editor.SetAttribute("aria-labelledby", LabelId);
        }

        if (disabled)
        {
            editor.SetAttribute("aria-disabled", "true");
        }

        // The content of a read-only editor isn't editable, so it would otherwise not be focusable
        if (readOnly)
        {
            editor.SetAttribute("aria-readonly", "true");
            editor.SetAttribute("tabindex", "0");
        }

        ApplyFieldAttributes(context, editor, FieldLayout.Stacked);

        var textarea = GenerateTextarea(htmlAttributes, rows);
        foreach (var name in new[] { "aria-labelledby", "aria-describedby", "aria-invalid" })
        {
            if (editor[name]?.Value?.ToString() is { } value)
            {
                textarea.MergeAttribute(name, value, replaceExisting: true);
            }
        }

        if (disabled)
        {
            textarea.MergeAttribute("disabled", "disabled");
        }

        if (readOnly)
        {
            textarea.MergeAttribute("readonly", "readonly");
        }

        var parts = new Parts(context, _iconOptions, id);
        if (!readOnly)
        {
            output.Content.AppendHtml(parts.Toolbar(ClassNames?.Toolbar, uploads));
        }

        var editorElement = new TagBuilder("div");
        foreach (var attribute in editor)
        {
            editorElement.MergeAttribute(attribute.Name, attribute.Value?.ToString());
        }

        output.Content.AppendHtml(editorElement);
        output.Content.AppendHtml(textarea);

        var note = new TagBuilder("p");
        note.Attributes.Add("data-slot", "rich-text-editor-note");
        note.Attributes.Add("class", "sa-rich-text-editor-note");
        note.Attributes.Add("aria-live", "polite");
        note.Attributes.Add("hidden", "hidden");
        output.Content.AppendHtml(note);

        if (editable)
        {
            output.Content.AppendHtml(parts.LinkPopover());
            output.Content.AppendHtml(parts.SlashMenu(uploads));
            output.Content.AppendHtml(parts.TableHandles());
            output.Content.AppendHtml(parts.Bubble());
        }

        output.Content.AppendHtml(ModuleScript.Create(ViewContext, "rich-text-editor.js"));

        return Task.FromResult(FieldLayout.Stacked);
    }

    private TagBuilder GenerateTextarea(IDictionary<string, object?>? htmlAttributes, int rows)
    {
        var textarea =
            For == null
                ? new TagBuilder("textarea")
                : _htmlGenerator.GenerateTextArea(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
                    rows,
                    columns: 0,
                    htmlAttributes: htmlAttributes
                );

        if (For == null)
        {
            if (htmlAttributes != null)
            {
                textarea.MergeAttributes(htmlAttributes, replaceExisting: true);
            }

            if (Name != null)
            {
                textarea.MergeAttribute("name", Name, replaceExisting: true);
            }

            textarea.MergeAttribute("rows", rows.ToString(CultureInfo.InvariantCulture));
            textarea.InnerHtml.Append(Value ?? string.Empty);
        }

        textarea.Attributes.Add("data-slot", "textarea");
        textarea.Attributes.Add("class", "sa-textarea");
        if (!string.IsNullOrEmpty(Placeholder))
        {
            textarea.MergeAttribute("placeholder", Placeholder, replaceExisting: true);
        }

        return textarea;
    }

    // Builds the editor's parts: the toolbar, link popover, slash menu, table handles and selection
    // bubble, drawn with the library's buttons, toggles, menus and tooltips. Each control names its
    // action in data-action, which the script runs and keeps checked or disabled.
    private sealed class Parts(TagHelperContext context, IconOptions iconOptions, string id)
    {
        public IHtmlContent Toolbar(string? toolbarClass, bool uploads)
        {
            var toolbar = new TagBuilder("div");
            toolbar.Attributes.Add("role", "toolbar");
            toolbar.Attributes.Add("aria-label", "Formatting");
            toolbar.Attributes.Add("data-slot", "rich-text-editor-toolbar");
            toolbar.Attributes.Add(
                "class",
                JoinCssClasses("sa-rich-text-editor-toolbar", toolbarClass)
            );

            var content = toolbar.InnerHtml;
            content.AppendHtml(Button("undo", "Undo", SemanticIconRole.RichTextUndo, "Mod Z"));
            content.AppendHtml(
                Button("redo", "Redo", SemanticIconRole.RichTextRedo, "Mod Shift Z")
            );
            content.AppendHtml(Separator());
            content.AppendHtml(BlockMenu());
            content.AppendHtml(Separator());
            content.AppendHtml(Toggle("bold", "Bold", SemanticIconRole.RichTextBold, "Mod B"));
            content.AppendHtml(
                Toggle("italic", "Italic", SemanticIconRole.RichTextItalic, "Mod I")
            );
            content.AppendHtml(
                Toggle("underline", "Underline", SemanticIconRole.RichTextUnderline, "Mod U")
            );
            content.AppendHtml(
                Toggle(
                    "strike",
                    "Strikethrough",
                    SemanticIconRole.RichTextStrikethrough,
                    "Mod Shift S"
                )
            );
            content.AppendHtml(
                Toggle("code", "Inline code", SemanticIconRole.RichTextCode, "Mod E")
            );
            content.AppendHtml(Separator());
            content.AppendHtml(
                Toggle("bullet-list", "Bulleted list", SemanticIconRole.RichTextBulletedList)
            );
            content.AppendHtml(
                Toggle("ordered-list", "Numbered list", SemanticIconRole.RichTextNumberedList)
            );
            content.AppendHtml(
                Toggle("blockquote", "Quote", SemanticIconRole.RichTextQuote, "Mod Shift B")
            );
            content.AppendHtml(
                Toggle("code-block", "Code block", SemanticIconRole.RichTextCodeBlock)
            );
            content.AppendHtml(Separator());
            content.AppendHtml(Toggle("link", "Link", SemanticIconRole.RichTextLink));
            if (uploads)
            {
                content.AppendHtml(Button("image", "Insert image", SemanticIconRole.RichTextImage));
            }

            content.AppendHtml(Button("table", "Insert table", SemanticIconRole.RichTextTable));
            content.AppendHtml(
                Button("horizontal-rule", "Insert divider", SemanticIconRole.RichTextDivider)
            );

            return toolbar;
        }

        public IHtmlContent LinkPopover()
        {
            var popover = new TagBuilder("div");
            popover.Attributes.Add("id", $"{id}-link");
            popover.Attributes.Add("popover", string.Empty);
            popover.Attributes.Add("data-slot", "rich-text-editor-link");
            popover.Attributes.Add("data-anchor-side", "bottom");
            popover.Attributes.Add(
                "class",
                $"sa-popover-content {AnchorPlacement.GetClassName("bottom", "start")}"
            );

            var form = new TagBuilder("div");
            form.Attributes.Add("class", "sa-rich-text-editor-link-form");

            var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.Attributes.Add("type", "url");
            input.Attributes.Add("aria-label", "Link address");
            input.Attributes.Add("placeholder", "https://");
            input.Attributes.Add("data-slot", "input");
            input.Attributes.Add("class", "sa-input");
            form.InnerHtml.AppendHtml(input);

            var apply = ButtonElement(ButtonVariant.Default, ButtonSize.Default);
            apply.Attributes.SetAttribute("data-link-action", "apply");
            apply.Content.Append("Apply");
            form.InnerHtml.AppendHtml(apply);

            var remove = ButtonElement(ButtonVariant.Ghost, ButtonSize.Icon);
            remove.Attributes.SetAttribute("data-link-action", "remove");
            remove.Attributes.SetAttribute("aria-label", "Remove link");
            remove.Content.AppendHtml(Icon(SemanticIconRole.RichTextUnlink));
            form.InnerHtml.AppendHtml(remove);

            popover.InnerHtml.AppendHtml(form);

            return popover;
        }

        public IHtmlContent SlashMenu(bool uploads)
        {
            var popup = new TagBuilder("prosekit-autocomplete-popup");
            popup.Attributes.Add("class", "sa-rich-text-editor-menu");

            var empty = new TagBuilder("prosekit-autocomplete-empty");
            empty.Attributes.Add("class", "sa-rich-text-editor-menu-empty");
            empty.InnerHtml.Append("No matching blocks");
            popup.InnerHtml.AppendHtml(empty);

            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "paragraph",
                    "Text",
                    SemanticIconRole.RichTextParagraph,
                    "text paragraph plain"
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "h1",
                    "Heading 1",
                    SemanticIconRole.RichTextHeading1,
                    "heading 1 h1 title",
                    "#"
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "h2",
                    "Heading 2",
                    SemanticIconRole.RichTextHeading2,
                    "heading 2 h2 subtitle",
                    "##"
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "h3",
                    "Heading 3",
                    SemanticIconRole.RichTextHeading3,
                    "heading 3 h3",
                    "###"
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "bullet-list",
                    "Bulleted list",
                    SemanticIconRole.RichTextBulletedList,
                    "bulleted list unordered ul",
                    "-"
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "ordered-list",
                    "Numbered list",
                    SemanticIconRole.RichTextNumberedList,
                    "numbered list ordered ol",
                    "1."
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "blockquote",
                    "Quote",
                    SemanticIconRole.RichTextQuote,
                    "quote blockquote",
                    ">"
                )
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "code-block",
                    "Code block",
                    SemanticIconRole.RichTextCodeBlock,
                    "code block pre",
                    "```"
                )
            );
            if (uploads)
            {
                popup.InnerHtml.AppendHtml(
                    SlashItem(
                        "image",
                        "Image",
                        SemanticIconRole.RichTextImage,
                        "image picture photo upload"
                    )
                );
            }

            popup.InnerHtml.AppendHtml(
                SlashItem("table", "Table", SemanticIconRole.RichTextTable, "table grid")
            );
            popup.InnerHtml.AppendHtml(
                SlashItem(
                    "horizontal-rule",
                    "Divider",
                    SemanticIconRole.RichTextDivider,
                    "divider horizontal rule hr line",
                    "---"
                )
            );

            var positioner = Positioner("prosekit-autocomplete-positioner", "bottom-start", popup);
            var root = new TagBuilder("prosekit-autocomplete-root");
            root.Attributes.Add("data-slot", "rich-text-editor-slash-menu");
            root.InnerHtml.AppendHtml(positioner);

            return root;
        }

        public IHtmlContent TableHandles()
        {
            var root = new TagBuilder("prosekit-table-handle-root");
            root.InnerHtml.AppendHtml(new TagBuilder("prosekit-table-handle-drag-preview"));
            root.InnerHtml.AppendHtml(new TagBuilder("prosekit-table-handle-drop-indicator"));
            root.InnerHtml.AppendHtml(TableHandle("column"));
            root.InnerHtml.AppendHtml(TableHandle("row"));

            return root;
        }

        public IHtmlContent Bubble()
        {
            var popup = new TagBuilder("prosekit-inline-popover-popup");
            popup.Attributes.Add("data-slot", "rich-text-editor-bubble");
            popup.Attributes.Add("class", "sa-rich-text-editor-bubble");
            popup.InnerHtml.AppendHtml(
                Toggle("bold", "Bold", SemanticIconRole.RichTextBold, tooltip: false)
            );
            popup.InnerHtml.AppendHtml(
                Toggle("italic", "Italic", SemanticIconRole.RichTextItalic, tooltip: false)
            );
            popup.InnerHtml.AppendHtml(
                Toggle("underline", "Underline", SemanticIconRole.RichTextUnderline, tooltip: false)
            );
            popup.InnerHtml.AppendHtml(
                Toggle("code", "Inline code", SemanticIconRole.RichTextCode, tooltip: false)
            );
            popup.InnerHtml.AppendHtml(Separator());
            popup.InnerHtml.AppendHtml(
                Toggle("link", "Link", SemanticIconRole.RichTextLink, tooltip: false)
            );

            var positioner = new TagBuilder("prosekit-inline-popover-positioner");
            positioner.Attributes.Add("placement", "top");
            positioner.Attributes.Add("offset", "8");
            positioner.InnerHtml.AppendHtml(popup);

            var root = new TagBuilder("prosekit-inline-popover-root");
            root.InnerHtml.AppendHtml(positioner);

            return root;
        }

        private IHtmlContent BlockMenu()
        {
            var menuId = $"{id}-block";
            var groupId = $"{id}-block-radio";

            var trigger = ButtonElement(ButtonVariant.Ghost, ButtonSize.Small);
            trigger.Attributes.SetAttribute("data-slot", "dropdown-menu-trigger");
            trigger.Attributes.SetAttribute("aria-haspopup", "menu");
            trigger.Attributes.SetAttribute("popovertarget", menuId);
            trigger.Attributes.SetAttribute("aria-label", "Text style");
            var label = new TagBuilder("span");
            label.Attributes.Add("data-slot", "rich-text-editor-block-label");
            label.Attributes.Add("data-default", "Text");
            label.InnerHtml.Append("Text");
            trigger.Content.AppendHtml(label);
            trigger.Content.AppendHtml(Icon(SemanticIconRole.DropdownIndicator));

            var group = new TagBuilder("div");
            group.Attributes.Add("role", "group");
            group.Attributes.Add("data-slot", "dropdown-menu-radio-group");
            group.Attributes.Add("data-radio-group", groupId);
            group.InnerHtml.AppendHtml(BlockItem(groupId, "paragraph", "Text", "Mod Alt 0"));
            group.InnerHtml.AppendHtml(BlockItem(groupId, "h1", "Heading 1", "Mod Alt 1"));
            group.InnerHtml.AppendHtml(BlockItem(groupId, "h2", "Heading 2", "Mod Alt 2"));
            group.InnerHtml.AppendHtml(BlockItem(groupId, "h3", "Heading 3", "Mod Alt 3"));

            var menu = new TagBuilder("sel-dropdown-menu");
            menu.Attributes.Add("id", menuId);
            menu.Attributes.Add("popover", string.Empty);
            menu.Attributes.Add("role", "menu");
            menu.Attributes.Add("data-slot", "dropdown-menu-content");
            menu.Attributes.Add("data-anchor-side", "bottom");
            menu.Attributes.Add("data-side", "bottom");
            menu.Attributes.Add(
                "class",
                "sa-dropdown-menu-content sa-dropdown-menu-content-logical "
                    + AnchorPlacement.GetClassName("bottom", "start")
            );
            var menuGroup = new TagBuilder("div");
            menuGroup.Attributes.Add("data-slot", "rich-text-editor-block-menu");
            menuGroup.InnerHtml.AppendHtml(group);
            menu.InnerHtml.AppendHtml(menuGroup);

            var content = new HtmlContentBuilder();
            content.AppendHtml(trigger);
            content.AppendHtml(menu);

            return content;
        }

        private IHtmlContent BlockItem(string groupId, string value, string text, string shortcut)
        {
            var item = new TagBuilder("div");
            item.Attributes.Add("role", "menuitemradio");
            item.Attributes.Add("tabindex", "-1");
            item.Attributes.Add("aria-checked", "false");
            item.Attributes.Add("data-slot", "dropdown-menu-radio-item");
            item.Attributes.Add("data-state", "unchecked");
            item.Attributes.Add("data-value", value);
            item.Attributes.Add("data-label", text);
            item.Attributes.Add("data-radio-group", groupId);
            item.Attributes.Add("data-close-on-click", "true");
            item.Attributes.Add("class", "sa-dropdown-menu-radio-item");

            var indicator = new TagBuilder("span");
            indicator.Attributes.Add("class", "sa-dropdown-menu-item-indicator");
            indicator.Attributes.Add("data-slot", "dropdown-menu-radio-item-indicator");
            indicator.InnerHtml.AppendHtml(Icon(SemanticIconRole.MenuItemSelected, "size-4"));
            item.InnerHtml.AppendHtml(indicator);
            item.InnerHtml.Append(text);
            item.InnerHtml.AppendHtml(Shortcut(shortcut));

            return item;
        }

        private TagHelperOutput Button(
            string action,
            string label,
            SemanticIconRole icon,
            string? shortcut = null
        )
        {
            var button = ButtonElement(ButtonVariant.Ghost, ButtonSize.IconSmall);
            button.Attributes.SetAttribute("aria-label", label);
            button.Attributes.SetAttribute("data-action", action);
            button.Attributes.SetAttribute("interestfor", TooltipId(action));
            button.Content.AppendHtml(Icon(icon));
            button.PostElement.AppendHtml(Tooltip(action, label, shortcut));

            return button;
        }

        private TagHelperOutput ButtonElement(ButtonVariant variant, ButtonSize size)
        {
            var button = new TagHelperOutput(
                "button",
                [new TagHelperAttribute("type", "button")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            ButtonRenderingHelper.RenderAttributes(button, variant, size);

            return button;
        }

        private IHtmlContent Icon(SemanticIconRole role, string? cssClass = null)
        {
            var output = new TagHelperOutput(
                "svg",
                cssClass == null
                    ? [new TagHelperAttribute("aria-hidden", "true")]
                    :
                    [
                        new TagHelperAttribute("class", cssClass),
                        new TagHelperAttribute("aria-hidden", "true"),
                    ],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            new IconTagHelper(iconOptions) { Name = iconOptions.GetSemanticIconName(role) }.Process(
                context,
                output
            );

            return output;
        }

        private TagBuilder MenuItem(
            string action,
            string text,
            SemanticIconRole icon,
            bool destructive = false
        )
        {
            var item = new TagBuilder("prosekit-menu-item");
            item.Attributes.Add("data-table-action", action);
            item.Attributes.Add("data-slot", "dropdown-menu-item");
            item.Attributes.Add("data-variant", destructive ? "destructive" : "default");
            item.Attributes.Add("class", "sa-dropdown-menu-item");
            item.InnerHtml.AppendHtml(Icon(icon));
            item.InnerHtml.Append(text);

            return item;
        }

        private TagBuilder MenuCheckItem(string action, string text, SemanticIconRole icon)
        {
            var item = new TagBuilder("prosekit-menu-item");
            item.Attributes.Add("data-table-action", action);
            item.Attributes.Add("data-state", "unchecked");
            item.Attributes.Add("data-slot", "dropdown-menu-checkbox-item");
            item.Attributes.Add("class", "sa-dropdown-menu-item sa-dropdown-menu-checkbox-item");
            item.InnerHtml.AppendHtml(Icon(icon));
            item.InnerHtml.Append(text);

            var indicator = new TagBuilder("span");
            indicator.Attributes.Add("class", "sa-dropdown-menu-item-indicator");
            indicator.Attributes.Add("data-slot", "dropdown-menu-checkbox-item-indicator");
            indicator.InnerHtml.AppendHtml(Icon(SemanticIconRole.MenuItemSelected, "size-4"));
            item.InnerHtml.AppendHtml(indicator);

            return item;
        }

        private static TagBuilder MenuSeparator()
        {
            var separator = new TagBuilder("div");
            separator.Attributes.Add("role", "separator");
            separator.Attributes.Add("aria-orientation", "horizontal");
            separator.Attributes.Add("data-slot", "dropdown-menu-separator");
            separator.Attributes.Add("class", "sa-dropdown-menu-separator");

            return separator;
        }

        private static TagBuilder Positioner(string tagName, string placement, IHtmlContent popup)
        {
            var positioner = new TagBuilder(tagName);
            positioner.Attributes.Add("placement", placement);
            positioner.Attributes.Add("offset", "6");
            positioner.InnerHtml.AppendHtml(popup);

            return positioner;
        }

        private static TagBuilder Separator()
        {
            var separator = new TagBuilder("div");
            separator.Attributes.Add("role", "separator");
            separator.Attributes.Add("aria-orientation", "vertical");
            separator.Attributes.Add("data-slot", "separator");
            separator.Attributes.Add("data-orientation", "vertical");
            separator.Attributes.Add("class", "sa-separator sa-separator-vertical");

            return separator;
        }

        // A key combination as keyboard keys. Mod is Ctrl here; the script shows Command on Apple
        // platforms.
        private static TagBuilder Shortcut(string shortcut, string slot = "dropdown-menu-shortcut")
        {
            var group = new TagBuilder(slot == "kbd-group" ? "kbd" : "span");
            group.Attributes.Add("data-slot", slot);
            group.Attributes.Add(
                "class",
                slot == "kbd-group" ? "sa-kbd-group" : "sa-dropdown-menu-shortcut"
            );
            foreach (var key in shortcut.Split(' '))
            {
                var kbd = new TagBuilder("kbd");
                kbd.Attributes.Add("data-slot", "kbd");
                kbd.Attributes.Add("class", "sa-kbd");
                if (key == "Mod")
                {
                    kbd.Attributes.Add("data-key", "mod");
                }

                kbd.InnerHtml.Append(key == "Mod" ? "Ctrl" : key);
                group.InnerHtml.AppendHtml(kbd);
            }

            return group;
        }

        private TagBuilder SlashItem(
            string name,
            string text,
            SemanticIconRole icon,
            string keywords,
            string? markdown = null
        )
        {
            var item = new TagBuilder("prosekit-autocomplete-item");
            item.Attributes.Add("value", keywords);
            item.Attributes.Add("data-slash", name);
            item.Attributes.Add("data-slot", "dropdown-menu-item");
            item.Attributes.Add("class", "sa-dropdown-menu-item");
            item.InnerHtml.AppendHtml(Icon(icon));
            item.InnerHtml.Append(text);
            if (markdown != null)
            {
                var shortcut = new TagBuilder("span");
                shortcut.Attributes.Add("data-slot", "dropdown-menu-shortcut");
                shortcut.Attributes.Add("class", "sa-dropdown-menu-shortcut");
                shortcut.InnerHtml.Append(markdown);
                item.InnerHtml.AppendHtml(shortcut);
            }

            return item;
        }

        private TagBuilder TableHandle(string axis)
        {
            var column = axis == "column";

            var popup = new TagBuilder("prosekit-menu-popup");
            popup.Attributes.Add("class", "sa-rich-text-editor-menu");
            popup.InnerHtml.AppendHtml(
                column
                    ? MenuItem(
                        "column-before",
                        "Insert column left",
                        SemanticIconRole.RichTextInsertColumnBefore
                    )
                    : MenuItem(
                        "row-above",
                        "Insert row above",
                        SemanticIconRole.RichTextInsertRowAbove
                    )
            );
            popup.InnerHtml.AppendHtml(
                column
                    ? MenuItem(
                        "column-after",
                        "Insert column right",
                        SemanticIconRole.RichTextInsertColumnAfter
                    )
                    : MenuItem(
                        "row-below",
                        "Insert row below",
                        SemanticIconRole.RichTextInsertRowBelow
                    )
            );
            popup.InnerHtml.AppendHtml(MenuSeparator());
            popup.InnerHtml.AppendHtml(
                column
                    ? MenuCheckItem(
                        "header-column",
                        "Header column",
                        SemanticIconRole.RichTextHeaderColumn
                    )
                    : MenuCheckItem("header-row", "Header row", SemanticIconRole.RichTextHeaderRow)
            );
            popup.InnerHtml.AppendHtml(
                MenuItem("merge", "Merge cells", SemanticIconRole.RichTextMergeCells)
            );
            popup.InnerHtml.AppendHtml(
                MenuItem("split", "Split cell", SemanticIconRole.RichTextSplitCell)
            );
            popup.InnerHtml.AppendHtml(
                MenuItem("clear", "Clear contents", SemanticIconRole.RichTextClearCells)
            );
            popup.InnerHtml.AppendHtml(MenuSeparator());
            popup.InnerHtml.AppendHtml(
                column
                    ? MenuItem(
                        "delete-column",
                        "Delete column",
                        SemanticIconRole.Remove,
                        destructive: true
                    )
                    : MenuItem(
                        "delete-row",
                        "Delete row",
                        SemanticIconRole.Remove,
                        destructive: true
                    )
            );
            popup.InnerHtml.AppendHtml(
                MenuItem("delete-table", "Delete table", SemanticIconRole.Remove, destructive: true)
            );

            var trigger = new TagBuilder($"prosekit-table-handle-{axis}-menu-trigger");
            trigger.Attributes.Add("aria-label", column ? "Column options" : "Row options");
            trigger.Attributes.Add("class", "sa-rich-text-editor-table-grip");
            trigger.InnerHtml.AppendHtml(
                Icon(
                    column
                        ? SemanticIconRole.RichTextColumnHandle
                        : SemanticIconRole.RichTextRowHandle
                )
            );

            var menuRoot = new TagBuilder($"prosekit-table-handle-{axis}-menu-root");
            menuRoot.InnerHtml.AppendHtml(trigger);
            menuRoot.InnerHtml.AppendHtml(
                Positioner(
                    "prosekit-menu-positioner",
                    column ? "bottom-start" : "right-start",
                    popup
                )
            );

            var handle = new TagBuilder($"prosekit-table-handle-{axis}-popup");
            handle.Attributes.Add("data-axis", axis);
            handle.Attributes.Add("class", "sa-rich-text-editor-table-handle");
            handle.InnerHtml.AppendHtml(menuRoot);

            var positioner = new TagBuilder($"prosekit-table-handle-{axis}-positioner");
            positioner.Attributes.Add("placement", column ? "top" : "left");
            positioner.InnerHtml.AppendHtml(handle);

            return positioner;
        }

        private TagHelperOutput Toggle(
            string action,
            string label,
            SemanticIconRole icon,
            string? shortcut = null,
            bool tooltip = true
        )
        {
            var toggle = new TagHelperOutput(
                "label",
                [],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            toggle.Attributes.SetAttribute("data-action", action);
            if (tooltip)
            {
                toggle.Attributes.SetAttribute("interestfor", TooltipId(action));
            }

            toggle.Attributes.SetAttribute("data-slot", "toggle");
            toggle.Attributes.SetAttribute("data-variant", "default");
            toggle.Attributes.SetAttribute("data-size", "sm");
            toggle.Attributes.SetAttribute(
                "class",
                ToggleRenderingHelper.BuildClass(
                    ToggleVariant.Default,
                    ToggleSize.Small,
                    false,
                    null
                )
            );

            var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.Attributes.Add("type", "checkbox");
            input.Attributes.Add("aria-label", label);
            input.Attributes.Add("data-slot", "toggle-input");
            input.Attributes.Add("class", "sr-only");
            toggle.Content.AppendHtml(input);
            toggle.Content.AppendHtml(Icon(icon));
            if (tooltip)
            {
                toggle.PostElement.AppendHtml(Tooltip(action, label, shortcut));
            }

            return toggle;
        }

        private TagBuilder Tooltip(string action, string label, string? shortcut)
        {
            var tooltip = new TagBuilder("div");
            tooltip.Attributes.Add("id", TooltipId(action));
            tooltip.Attributes.Add("popover", "hint");
            tooltip.Attributes.Add("data-slot", "tooltip-content");
            tooltip.Attributes.Add("data-anchor-side", "bottom");
            tooltip.Attributes.Add(
                "class",
                $"sa-tooltip-content {AnchorPlacement.GetClassName("bottom", "center")}"
            );
            tooltip.InnerHtml.Append(label);
            if (shortcut != null)
            {
                tooltip.InnerHtml.Append(" ");
                tooltip.InnerHtml.AppendHtml(Shortcut(shortcut, "kbd-group"));
            }

            return tooltip;
        }

        private string TooltipId(string action) => $"{id}-tip-{action}";
    }
}
