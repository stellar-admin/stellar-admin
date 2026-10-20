namespace StellarAdmin.UI.Icons;

/// <summary>
///     Supplies the Lucide icons.
/// </summary>
public sealed class LucideIconPack : IIconPack
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, IconDefinition> GetIcons()
    {
        return LucideIcons.IconDefinitions;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
    {
        return new Dictionary<SemanticIconRole, string>
        {
            [SemanticIconRole.AccordionIndicator] = "chevron-down",
            [SemanticIconRole.BooleanFalse] = "x",
            [SemanticIconRole.BooleanTrue] = "check",
            [SemanticIconRole.BreadcrumbEllipsis] = "ellipsis",
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
            [SemanticIconRole.Error] = "octagon-x",
            [SemanticIconRole.Info] = "info",
            [SemanticIconRole.Loading] = "loader-circle",
            [SemanticIconRole.MenuItemSelected] = "check",
            [SemanticIconRole.OtpSeparator] = "minus",
            [SemanticIconRole.PaginationEllipsis] = "ellipsis",
            [SemanticIconRole.PaginationFirst] = "chevron-first",
            [SemanticIconRole.PaginationLast] = "chevron-last",
            [SemanticIconRole.PaginationNext] = "chevron-right",
            [SemanticIconRole.PaginationPrevious] = "chevron-left",
            [SemanticIconRole.RadioSelected] = "circle",
            [SemanticIconRole.RowDetailIndicator] = "chevron-right",
            [SemanticIconRole.ScrollToEnd] = "arrow-down",
            [SemanticIconRole.Search] = "search",
            [SemanticIconRole.SortAscending] = "arrow-up",
            [SemanticIconRole.SortDescending] = "arrow-down",
            [SemanticIconRole.SortUnsorted] = "chevrons-up-down",
            [SemanticIconRole.SubmenuIndicator] = "chevron-right",
            [SemanticIconRole.Success] = "circle-check",
            [SemanticIconRole.ToggleSidebar] = "panel-left",
            [SemanticIconRole.Image] = "image",
            [SemanticIconRole.ImageError] = "image-off",
            [SemanticIconRole.Upload] = "upload",
            [SemanticIconRole.Remove] = "trash",
            [SemanticIconRole.Warning] = "triangle-alert",
        };
    }
}
