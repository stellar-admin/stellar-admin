namespace StellarAdmin.Icons;

public class TablerFilledIconPack : IIconPack
{
    /// <inheritdoc/>
    public IDictionary<string, IconDefinition> GetIcons()
    {
        return TablerFilledIcons.IconDefinitions;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
    {
        return new Dictionary<SemanticIconRole, string>
        {
            [SemanticIconRole.AccordionIndicator] = "chevron-down",
            [SemanticIconRole.BreadcrumbEllipsis] = "dots",
            [SemanticIconRole.BreadcrumbSeparator] = "chevron-right",
            [SemanticIconRole.CarouselNext] = "caret-right",
            [SemanticIconRole.CarouselPrevious] = "caret-left",
            [SemanticIconRole.CheckboxSelected] = "check",
            [SemanticIconRole.ChoiceSelected] = "check",
            [SemanticIconRole.Close] = "x",
            [SemanticIconRole.DropdownIndicator] = "chevron-down",
            [SemanticIconRole.Loading] = "inner-shadow-top",
            [SemanticIconRole.MenuItemSelected] = "check",
            [SemanticIconRole.OtpSeparator] = "point",
            [SemanticIconRole.PaginationEllipsis] = "dots",
            [SemanticIconRole.PaginationFirst] = "player-skip-back",
            [SemanticIconRole.PaginationLast] = "player-skip-forward",
            [SemanticIconRole.PaginationNext] = "caret-right",
            [SemanticIconRole.PaginationPrevious] = "caret-left",
            [SemanticIconRole.RadioSelected] = "circle",
            [SemanticIconRole.ScrollToEnd] = "arrow-big-down",
            [SemanticIconRole.SortAscending] = "caret-up",
            [SemanticIconRole.SortDescending] = "caret-down",
            [SemanticIconRole.SortUnsorted] = "caret-up-down",
            [SemanticIconRole.SubmenuIndicator] = "chevron-right",
            [SemanticIconRole.ToggleSidebar] = "layout-sidebar",
        };
    }
}
