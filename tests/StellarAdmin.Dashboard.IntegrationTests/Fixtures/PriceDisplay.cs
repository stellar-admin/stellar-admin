using StellarAdmin.Dashboard.Resources.Displays;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class PriceDisplay : FieldDisplay
{
    public string Currency { get; set; } = "EUR";

    public override string TemplateName => "Price";
}
