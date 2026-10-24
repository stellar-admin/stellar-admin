namespace StellarAdmin.UI.Icons;

/// <summary>
///     Specifies the purpose of an icon that a built-in component renders.
/// </summary>
public enum SemanticIconRole
{
    /// <summary>
    ///     Omitted breadcrumb levels.
    /// </summary>
    BreadcrumbEllipsis,

    /// <summary>
    ///     The separator between breadcrumb levels.
    /// </summary>
    BreadcrumbSeparator,

    /// <summary>
    ///     Navigation to the next carousel slide.
    /// </summary>
    CarouselNext,

    /// <summary>
    ///     Navigation to the previous carousel slide.
    /// </summary>
    CarouselPrevious,

    /// <summary>
    ///     A selected checkbox.
    /// </summary>
    CheckboxSelected,

    /// <summary>
    ///     A selected questionnaire choice.
    /// </summary>
    ChoiceSelected,

    /// <summary>
    ///     An action that dismisses a surface.
    /// </summary>
    Close,

    /// <summary>
    ///     An indicator that a select can open.
    /// </summary>
    DropdownIndicator,

    /// <summary>
    ///     An animated loading indicator.
    /// </summary>
    Loading,

    /// <summary>
    ///     A selected checkbox or radio menu item.
    /// </summary>
    MenuItemSelected,

    /// <summary>
    ///     The separator between one-time code groups.
    /// </summary>
    OtpSeparator,

    /// <summary>
    ///     Omitted pages in pagination.
    /// </summary>
    PaginationEllipsis,

    /// <summary>
    ///     Navigation to the first page.
    /// </summary>
    PaginationFirst,

    /// <summary>
    ///     Navigation to the last page.
    /// </summary>
    PaginationLast,

    /// <summary>
    ///     Navigation to the next page.
    /// </summary>
    PaginationNext,

    /// <summary>
    ///     Navigation to the previous page.
    /// </summary>
    PaginationPrevious,

    /// <summary>
    ///     A selected radio input.
    /// </summary>
    RadioSelected,

    /// <summary>
    ///     Navigation to the end of a scrolling message list.
    /// </summary>
    ScrollToEnd,

    /// <summary>
    ///     An ascending sort order.
    /// </summary>
    SortAscending,

    /// <summary>
    ///     A descending sort order.
    /// </summary>
    SortDescending,

    /// <summary>
    ///     A sortable column without an active sort direction.
    /// </summary>
    SortUnsorted,

    /// <summary>
    ///     An indicator that a nested menu can open.
    /// </summary>
    SubmenuIndicator,

    /// <summary>
    ///     An action that toggles the sidebar.
    /// </summary>
    ToggleSidebar,

    /// <summary>
    ///     An indicator that an accordion item can expand.
    /// </summary>
    AccordionIndicator,

    /// <summary>
    ///     A search field.
    /// </summary>
    Search,

    /// <summary>
    ///     A successful outcome.
    /// </summary>
    Success,

    /// <summary>
    ///     An informational message.
    /// </summary>
    Info,

    /// <summary>
    ///     A message that needs attention.
    /// </summary>
    Warning,

    /// <summary>
    ///     A failed operation.
    /// </summary>
    Error,

    /// <summary>
    ///     An indicator that a table row can expand to show its details.
    /// </summary>
    RowDetailIndicator,

    /// <summary>
    ///     A true value in a display field.
    /// </summary>
    BooleanTrue,

    /// <summary>
    ///     A false value in a display field.
    /// </summary>
    BooleanFalse,

    /// <summary>
    ///     A missing image.
    /// </summary>
    Image,

    /// <summary>
    ///     An image that couldn't be loaded.
    /// </summary>
    ImageError,

    /// <summary>
    ///     An action that uploads a file.
    /// </summary>
    Upload,

    /// <summary>
    ///     An action that removes a value.
    /// </summary>
    Remove,

    /// <summary>
    ///     A checklist item that has not been started.
    /// </summary>
    ChecklistTodo,

    /// <summary>
    ///     A checklist item that has been started but is not finished.
    /// </summary>
    ChecklistInProgress,

    /// <summary>
    ///     A finished checklist item.
    /// </summary>
    ChecklistDone,

    /// <summary>
    ///     An action that undoes the last change in a rich text editor.
    /// </summary>
    RichTextUndo,

    /// <summary>
    ///     An action that redoes the last undone change in a rich text editor.
    /// </summary>
    RichTextRedo,

    /// <summary>
    ///     Bold text in a rich text editor.
    /// </summary>
    RichTextBold,

    /// <summary>
    ///     Italic text in a rich text editor.
    /// </summary>
    RichTextItalic,

    /// <summary>
    ///     Underlined text in a rich text editor.
    /// </summary>
    RichTextUnderline,

    /// <summary>
    ///     Struck-through text in a rich text editor.
    /// </summary>
    RichTextStrikethrough,

    /// <summary>
    ///     Inline code in a rich text editor.
    /// </summary>
    RichTextCode,

    /// <summary>
    ///     A bulleted list in a rich text editor.
    /// </summary>
    RichTextBulletedList,

    /// <summary>
    ///     A numbered list in a rich text editor.
    /// </summary>
    RichTextNumberedList,

    /// <summary>
    ///     A quote in a rich text editor.
    /// </summary>
    RichTextQuote,

    /// <summary>
    ///     A code block in a rich text editor.
    /// </summary>
    RichTextCodeBlock,

    /// <summary>
    ///     A link in a rich text editor.
    /// </summary>
    RichTextLink,

    /// <summary>
    ///     An action that removes a link in a rich text editor.
    /// </summary>
    RichTextUnlink,

    /// <summary>
    ///     An action that inserts an image in a rich text editor.
    /// </summary>
    RichTextImage,

    /// <summary>
    ///     An action that inserts a table in a rich text editor.
    /// </summary>
    RichTextTable,

    /// <summary>
    ///     An action that inserts a divider in a rich text editor.
    /// </summary>
    RichTextDivider,

    /// <summary>
    ///     Plain paragraph text in a rich text editor.
    /// </summary>
    RichTextParagraph,

    /// <summary>
    ///     A first-level heading in a rich text editor.
    /// </summary>
    RichTextHeading1,

    /// <summary>
    ///     A second-level heading in a rich text editor.
    /// </summary>
    RichTextHeading2,

    /// <summary>
    ///     A third-level heading in a rich text editor.
    /// </summary>
    RichTextHeading3,

    /// <summary>
    ///     The handle that selects, moves and opens the menu of a table column in a rich text editor.
    /// </summary>
    RichTextColumnHandle,

    /// <summary>
    ///     The handle that selects, moves and opens the menu of a table row in a rich text editor.
    /// </summary>
    RichTextRowHandle,

    /// <summary>
    ///     An action that inserts a table column before the current one in a rich text editor.
    /// </summary>
    RichTextInsertColumnBefore,

    /// <summary>
    ///     An action that inserts a table column after the current one in a rich text editor.
    /// </summary>
    RichTextInsertColumnAfter,

    /// <summary>
    ///     An action that inserts a table row above the current one in a rich text editor.
    /// </summary>
    RichTextInsertRowAbove,

    /// <summary>
    ///     An action that inserts a table row below the current one in a rich text editor.
    /// </summary>
    RichTextInsertRowBelow,

    /// <summary>
    ///     A table's header column in a rich text editor.
    /// </summary>
    RichTextHeaderColumn,

    /// <summary>
    ///     A table's header row in a rich text editor.
    /// </summary>
    RichTextHeaderRow,

    /// <summary>
    ///     An action that merges table cells in a rich text editor.
    /// </summary>
    RichTextMergeCells,

    /// <summary>
    ///     An action that splits a merged table cell in a rich text editor.
    /// </summary>
    RichTextSplitCell,

    /// <summary>
    ///     An action that clears the contents of table cells in a rich text editor.
    /// </summary>
    RichTextClearCells,

    /// <summary>
    ///     An action that adds an item to a list or dictionary editor.
    /// </summary>
    CollectionEditorAdd,

    /// <summary>
    ///     An action that removes an item from a list or dictionary editor.
    /// </summary>
    CollectionEditorRemove,

    /// <summary>
    ///     The handle that moves an item in a list editor.
    /// </summary>
    CollectionEditorHandle,
}
