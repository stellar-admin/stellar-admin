using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.TagHelpers.Tests.Toasts;

public class TempDataToastNotifierTests
{
    [Test]
    public async Task Add_WhenDurationIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        await using var provider = CreateServices();
        var sut = provider.GetRequiredService<IToastNotifier>();
        var toast = new Toast { Title = "Booking saved", Duration = TimeSpan.FromSeconds(-1) };

        // Act
        Action act = () => sut.Add(toast);

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Add_WhenTitleIsBlank_ThrowsArgumentException(string title)
    {
        // Arrange
        await using var provider = CreateServices();
        var sut = provider.GetRequiredService<IToastNotifier>();
        var toast = new Toast { Title = title };

        // Act
        Action act = () => sut.Add(toast);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Add_WhenNoRequestIsActive_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var provider = CreateServices();
        var sut = provider.GetRequiredService<IToastNotifier>();
        var toast = new Toast { Title = "Booking saved" };

        // Act
        Action act = () => sut.Add(toast);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddControllersWithViews();
        services.AddStellarAdmin().AddTagHelpers();

        return services.BuildServiceProvider();
    }
}
