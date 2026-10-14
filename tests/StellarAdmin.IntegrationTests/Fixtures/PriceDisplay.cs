using StellarAdmin.Resources.Displays;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class PriceDisplay : FieldDisplay
{
    public string Currency { get; set; } = "EUR";

    public override string TemplateName => "Price";
}
