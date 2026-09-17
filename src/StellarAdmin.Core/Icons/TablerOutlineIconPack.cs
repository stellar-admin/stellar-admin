namespace StellarAdmin.Icons;

public class TablerOutlineIconPack : IIconPack
{
    /// <inheritdoc/>
    public IDictionary<string, IconDefinition> GetIcons()
    {
        return TablerOutlineIcons.IconDefinitions;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
    {
        return new Dictionary<SemanticIconRole, string>
        {
            [SemanticIconRole.AccordionIndicator] = "chevron-down",
            [SemanticIconRole.BreadcrumbEllipsis] = "dots",
            [SemanticIconRole.BreadcrumbSeparator] = "chevron-right",
            [SemanticIconRole.CarouselNext] = "chevron-right",
            [SemanticIconRole.CarouselPrevious] = "chevron-left",
            [SemanticIconRole.CheckboxSelected] = "check",
            [SemanticIconRole.ChoiceSelected] = "check",
            [SemanticIconRole.Close] = "x",
            [SemanticIconRole.DropdownIndicator] = "chevron-down",
            [SemanticIconRole.Loading] = "loader-2",
            [SemanticIconRole.MenuItemSelected] = "check",
            [SemanticIconRole.OtpSeparator] = "minus",
            [SemanticIconRole.PaginationEllipsis] = "dots",
            [SemanticIconRole.PaginationFirst] = "chevron-left-pipe",
            [SemanticIconRole.PaginationLast] = "chevron-right-pipe",
            [SemanticIconRole.PaginationNext] = "chevron-right",
            [SemanticIconRole.PaginationPrevious] = "chevron-left",
            [SemanticIconRole.RadioSelected] = "circle",
            [SemanticIconRole.ScrollToEnd] = "arrow-down",
            [SemanticIconRole.SortAscending] = "arrow-up",
            [SemanticIconRole.SortDescending] = "arrow-down",
            [SemanticIconRole.SortUnsorted] = "selector",
            [SemanticIconRole.SubmenuIndicator] = "chevron-right",
            [SemanticIconRole.ToggleSidebar] = "layout-sidebar",
        };
    }
}
