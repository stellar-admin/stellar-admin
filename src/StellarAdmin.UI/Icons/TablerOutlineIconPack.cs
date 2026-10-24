namespace StellarAdmin.UI.Icons;

/// <summary>
///     Supplies the outline Tabler icons.
/// </summary>
public sealed class TablerOutlineIconPack : IIconPack
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, IconDefinition> GetIcons()
    {
        return TablerOutlineIcons.IconDefinitions;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
    {
        return new Dictionary<SemanticIconRole, string>
        {
            [SemanticIconRole.AccordionIndicator] = "chevron-down",
            [SemanticIconRole.BooleanFalse] = "x",
            [SemanticIconRole.BooleanTrue] = "check",
            [SemanticIconRole.BreadcrumbEllipsis] = "dots",
            [SemanticIconRole.BreadcrumbSeparator] = "chevron-right",
            [SemanticIconRole.CarouselNext] = "chevron-right",
            [SemanticIconRole.CarouselPrevious] = "chevron-left",
            [SemanticIconRole.ChecklistDone] = "circle-check",
            [SemanticIconRole.ChecklistInProgress] = "circle-dot",
            [SemanticIconRole.ChecklistTodo] = "circle",
            [SemanticIconRole.CheckboxSelected] = "check",
            [SemanticIconRole.ChoiceSelected] = "check",
            [SemanticIconRole.Close] = "x",
            [SemanticIconRole.DropdownIndicator] = "chevron-down",
            [SemanticIconRole.Error] = "alert-octagon",
            [SemanticIconRole.Info] = "info-circle",
            [SemanticIconRole.Loading] = "loader-2",
            [SemanticIconRole.MenuItemSelected] = "check",
            [SemanticIconRole.OtpSeparator] = "minus",
            [SemanticIconRole.PaginationEllipsis] = "dots",
            [SemanticIconRole.PaginationFirst] = "chevron-left-pipe",
            [SemanticIconRole.PaginationLast] = "chevron-right-pipe",
            [SemanticIconRole.PaginationNext] = "chevron-right",
            [SemanticIconRole.PaginationPrevious] = "chevron-left",
            [SemanticIconRole.RadioSelected] = "circle",
            [SemanticIconRole.RowDetailIndicator] = "chevron-right",
            [SemanticIconRole.ScrollToEnd] = "arrow-down",
            [SemanticIconRole.Search] = "search",
            [SemanticIconRole.SortAscending] = "arrow-up",
            [SemanticIconRole.SortDescending] = "arrow-down",
            [SemanticIconRole.SortUnsorted] = "selector",
            [SemanticIconRole.SubmenuIndicator] = "chevron-right",
            [SemanticIconRole.Success] = "circle-check",
            [SemanticIconRole.ToggleSidebar] = "layout-sidebar",
            [SemanticIconRole.Image] = "photo",
            [SemanticIconRole.ImageError] = "photo-off",
            [SemanticIconRole.Upload] = "upload",
            [SemanticIconRole.Remove] = "trash",
            [SemanticIconRole.Warning] = "alert-triangle",
            [SemanticIconRole.RichTextUndo] = "arrow-back-up",
            [SemanticIconRole.RichTextRedo] = "arrow-forward-up",
            [SemanticIconRole.RichTextBold] = "bold",
            [SemanticIconRole.RichTextItalic] = "italic",
            [SemanticIconRole.RichTextUnderline] = "underline",
            [SemanticIconRole.RichTextStrikethrough] = "strikethrough",
            [SemanticIconRole.RichTextCode] = "code",
            [SemanticIconRole.RichTextBulletedList] = "list",
            [SemanticIconRole.RichTextNumberedList] = "list-numbers",
            [SemanticIconRole.RichTextQuote] = "blockquote",
            [SemanticIconRole.RichTextCodeBlock] = "source-code",
            [SemanticIconRole.RichTextLink] = "link",
            [SemanticIconRole.RichTextUnlink] = "unlink",
            [SemanticIconRole.RichTextImage] = "photo-plus",
            [SemanticIconRole.RichTextTable] = "table",
            [SemanticIconRole.RichTextDivider] = "separator-horizontal",
            [SemanticIconRole.RichTextParagraph] = "pilcrow",
            [SemanticIconRole.RichTextHeading1] = "h-1",
            [SemanticIconRole.RichTextHeading2] = "h-2",
            [SemanticIconRole.RichTextHeading3] = "h-3",
            [SemanticIconRole.RichTextColumnHandle] = "grip-horizontal",
            [SemanticIconRole.RichTextRowHandle] = "grip-vertical",
            [SemanticIconRole.RichTextInsertColumnBefore] = "column-insert-left",
            [SemanticIconRole.RichTextInsertColumnAfter] = "column-insert-right",
            [SemanticIconRole.RichTextInsertRowAbove] = "row-insert-top",
            [SemanticIconRole.RichTextInsertRowBelow] = "row-insert-bottom",
            [SemanticIconRole.RichTextHeaderColumn] = "table-column",
            [SemanticIconRole.RichTextHeaderRow] = "table-row",
            [SemanticIconRole.RichTextMergeCells] = "arrows-join",
            [SemanticIconRole.RichTextSplitCell] = "arrows-split",
            [SemanticIconRole.RichTextClearCells] = "eraser",
            [SemanticIconRole.CollectionEditorAdd] = "plus",
            [SemanticIconRole.CollectionEditorRemove] = "x",
            [SemanticIconRole.CollectionEditorHandle] = "grip-vertical",
        };
    }
}
