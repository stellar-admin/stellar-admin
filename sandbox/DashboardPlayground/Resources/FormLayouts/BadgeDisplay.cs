using StellarAdmin.Dashboard.Resources.Displays;
using StellarAdmin.TagHelpers;

namespace DashboardPlayground.Resources.FormLayouts;

// A custom display: UseDisplay<BadgeDisplay> passes the variant to Views/Shared/DisplayTemplates/Badge.cshtml.
public sealed class BadgeDisplay : FieldDisplay
{
    public BadgeVariant Variant { get; set; } = BadgeVariant.Secondary;

    public override string TemplateName => "Badge";
}
