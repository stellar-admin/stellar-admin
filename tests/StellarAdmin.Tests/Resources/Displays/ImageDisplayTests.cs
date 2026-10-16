using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Tests.Resources.Displays;

public class ImageDisplayTests
{
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public async Task ResolveUrlAsync_WithEmptyValue_ReturnsNull(string? value)
    {
        // Arrange
        var display = new ImageDisplay();
        display.ResolveUrl(stored => $"https://cdn.voyager.example/{stored}");

        // Act
        var url = await display.ResolveUrlAsync(value, Services, CancellationToken.None);

        // Assert
        await Assert.That(url).IsNull();
    }

    [Test]
    public async Task ResolveUrlAsync_WithoutResolver_ReturnsValue()
    {
        // Arrange
        var display = new ImageDisplay();

        // Act
        var url = await display.ResolveUrlAsync(
            "https://cdn.voyager.example/cape-town.jpg",
            Services,
            CancellationToken.None
        );

        // Assert
        await Assert.That(url).IsEqualTo("https://cdn.voyager.example/cape-town.jpg");
    }

    [Test]
    public async Task ResolveUrlAsync_WithResolver_ReturnsResolvedUrl()
    {
        // Arrange
        var display = new ImageDisplay();
        display.ResolveUrl(
            (stored, services, _) =>
                Task.FromResult<string?>(services.GetRequiredService<string>() + stored)
        );
        var services = new ServiceCollection()
            .AddSingleton("https://cdn.voyager.example/")
            .BuildServiceProvider();

        // Act
        var url = await display.ResolveUrlAsync("cape-town.jpg", services, CancellationToken.None);

        // Assert
        await Assert.That(url).IsEqualTo("https://cdn.voyager.example/cape-town.jpg");
    }

    private static IServiceProvider Services { get; } =
        new ServiceCollection().BuildServiceProvider();
}
