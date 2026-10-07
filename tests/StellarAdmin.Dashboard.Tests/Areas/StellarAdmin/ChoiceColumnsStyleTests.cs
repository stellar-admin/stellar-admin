using StellarAdmin.Dashboard.Areas.StellarAdmin;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Areas.StellarAdmin;

public class ChoiceColumnsStyleTests
{
    [Test]
    public async Task Format_Across_WritesOnlyColumns()
    {
        // Arrange
        var columns = new FormGridTiers(1, 2, 2, 3);

        // Act
        var style = ChoiceColumnsStyle.Format(columns, 7, false);

        // Assert
        await Assert.That(style).IsEqualTo("--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:3");
    }

    [Test]
    public async Task Format_Down_WritesRowsThatFillEachColumnFirst()
    {
        // Arrange
        var columns = new FormGridTiers(1, 2, 2, 3);

        // Act
        var style = ChoiceColumnsStyle.Format(columns, 7, true);

        // Assert
        await Assert
            .That(style)
            .IsEqualTo(
                "--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:3;--sa-rows:7;--sa-rows-sm:4;--sa-rows-md:4;--sa-rows-lg:3"
            );
    }

    [Test]
    public async Task Format_DownWithOneRowPerTier_WritesOnlyColumns()
    {
        // Arrange
        var columns = new FormGridTiers(3, 3, 3, 3);

        // Act
        var style = ChoiceColumnsStyle.Format(columns, 3, true);

        // Assert
        await Assert
            .That(style)
            .IsEqualTo("--sa-cols:3;--sa-cols-sm:3;--sa-cols-md:3;--sa-cols-lg:3");
    }

    [Test]
    public async Task Format_DownWithoutChoices_WritesNothing()
    {
        // Arrange
        var columns = new FormGridTiers(1, 1, 1, 1);

        // Act
        var style = ChoiceColumnsStyle.Format(columns, 0, true);

        // Assert
        await Assert.That(style).IsNull();
    }
}
