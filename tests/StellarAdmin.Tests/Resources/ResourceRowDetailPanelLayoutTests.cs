using StellarAdmin.Resources;

namespace StellarAdmin.Tests.Resources;

public class ResourceRowDetailPanelLayoutTests
{
    [Test]
    public async Task Accordion_WithoutExpandMode_AllowsMultiplePanelsOpen()
    {
        // Act
        var result = ResourceRowDetailPanelLayout.Accordion();

        // Assert
        await Assert
            .That(result.ExpandMode)
            .IsEqualTo(ResourceRowDetailAccordionExpandMode.Multiple);
        await Assert.That(result.Problem).IsNull();
    }

    [Test]
    public async Task Accordion_WithUndefinedExpandMode_RecordsProblem()
    {
        // Act
        var result = ResourceRowDetailPanelLayout.Accordion(
            (ResourceRowDetailAccordionExpandMode)99
        );

        // Assert
        await Assert.That(result.Problem).IsNotNull();
    }

    [Test]
    public async Task Tabs_WithoutStyle_UsesDefaultStyle()
    {
        // Act
        var result = ResourceRowDetailPanelLayout.Tabs();

        // Assert
        await Assert.That(result.Style).IsEqualTo(ResourceRowDetailTabStyle.Default);
        await Assert.That(result.Problem).IsNull();
    }

    [Test]
    public async Task Tabs_WithUndefinedStyle_RecordsProblem()
    {
        // Act
        var result = ResourceRowDetailPanelLayout.Tabs((ResourceRowDetailTabStyle)99);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
    }
}
