namespace StellarAdmin.UI.Tests;

public class DisplayNamesTests
{
    [Test]
    [Arguments("DepartureDate", "Departure Date")]
    [Arguments("Name", "Name")]
    [Arguments("HTMLCode", "HTML Code")]
    [Arguments("CategoryId", "Category Id")]
    [Arguments("name", "name")]
    public async Task FromPropertyName_PascalCaseName_SplitsIntoWords(string name, string expected)
    {
        // Arrange

        // Act
        var displayName = DisplayNames.FromPropertyName(name);

        // Assert
        await Assert.That(displayName).IsEqualTo(expected);
    }
}
