namespace StellarAdmin.Icons;

public class LucideIconPack : IIconPack
{
    public IDictionary<string, IconDefinition> GetIcons()
    {
        return LucideIcons.IconDefinitions;
    }

    /// <summary>
    ///     Returns the Lucide icon names used for semantic roles.
    /// </summary>
    public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
    {
        return new Dictionary<SemanticIconRole, string>
        {
            [SemanticIconRole.AccordionIndicator] = "chevron-down",
            [SemanticIconRole.BreadcrumbEllipsis] = "ellipsis",
            [SemanticIconRole.BreadcrumbSeparator] = "chevron-right",
            [SemanticIconRole.CarouselNext] = "chevron-right",
            [SemanticIconRole.CarouselPrevious] = "chevron-left",
            [SemanticIconRole.CheckboxSelected] = "check",
            [SemanticIconRole.ChoiceSelected] = "check",
            [SemanticIconRole.Close] = "x",
            [SemanticIconRole.DropdownIndicator] = "chevron-down",
            [SemanticIconRole.Loading] = "loader-circle",
            [SemanticIconRole.MenuItemSelected] = "check",
            [SemanticIconRole.OtpSeparator] = "minus",
            [SemanticIconRole.PaginationEllipsis] = "ellipsis",
            [SemanticIconRole.PaginationFirst] = "chevron-first",
            [SemanticIconRole.PaginationLast] = "chevron-last",
            [SemanticIconRole.PaginationNext] = "chevron-right",
            [SemanticIconRole.PaginationPrevious] = "chevron-left",
            [SemanticIconRole.RadioSelected] = "circle",
            [SemanticIconRole.ScrollToEnd] = "arrow-down",
            [SemanticIconRole.SortAscending] = "arrow-up",
            [SemanticIconRole.SortDescending] = "arrow-down",
            [SemanticIconRole.SortUnsorted] = "chevrons-up-down",
            [SemanticIconRole.SubmenuIndicator] = "chevron-right",
            [SemanticIconRole.ToggleSidebar] = "panel-left",
        };
    }
}
