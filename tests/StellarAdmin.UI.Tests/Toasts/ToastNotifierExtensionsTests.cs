using StellarAdmin.UI.Toasts;

namespace StellarAdmin.UI.Tests.Toasts;

public class ToastNotifierExtensionsTests
{
    public static IEnumerable<Func<(ToastType, Action<IToastNotifier, string?>)>> Shortcuts()
    {
        yield return () =>
            (
                ToastType.Error,
                (notifier, description) => notifier.Error("Booking failed", description!)
            );
        yield return () =>
            (
                ToastType.Info,
                (notifier, description) => notifier.Info("Booking failed", description!)
            );
        yield return () =>
            (
                ToastType.Success,
                (notifier, description) => notifier.Success("Booking failed", description!)
            );
        yield return () =>
            (
                ToastType.Warning,
                (notifier, description) => notifier.Warning("Booking failed", description!)
            );
    }

    public static IEnumerable<Func<(ToastType, Action<IToastNotifier>)>> TitleOnlyShortcuts()
    {
        yield return () => (ToastType.Error, notifier => notifier.Error("Booking failed"));
        yield return () => (ToastType.Info, notifier => notifier.Info("Booking failed"));
        yield return () => (ToastType.Success, notifier => notifier.Success("Booking failed"));
        yield return () => (ToastType.Warning, notifier => notifier.Warning("Booking failed"));
    }

    [Test]
    [MethodDataSource(nameof(Shortcuts))]
    public async Task Shortcut_WithDescription_AddsToastWithDescription(
        ToastType type,
        Action<IToastNotifier, string?> shortcut
    )
    {
        // Arrange
        var sut = new RecordingToastNotifier();

        // Act
        shortcut(sut, "Try another date.");

        // Assert
        await Assert.That(sut.Toasts).Count().IsEqualTo(1);
        await Assert.That(sut.Toasts[0].Type).IsEqualTo(type);
        await Assert.That(sut.Toasts[0].Description).IsEqualTo("Try another date.");
    }

    [Test]
    [MethodDataSource(nameof(Shortcuts))]
    public async Task Shortcut_WithNullDescription_ThrowsArgumentNullException(
        ToastType type,
        Action<IToastNotifier, string?> shortcut
    )
    {
        // Arrange
        var sut = new RecordingToastNotifier();

        // Act
        Action act = () => shortcut(sut, null);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    [MethodDataSource(nameof(TitleOnlyShortcuts))]
    public async Task Shortcut_WithoutDescription_AddsToastWithoutDescription(
        ToastType type,
        Action<IToastNotifier> shortcut
    )
    {
        // Arrange
        var sut = new RecordingToastNotifier();

        // Act
        shortcut(sut);

        // Assert
        await Assert.That(sut.Toasts).Count().IsEqualTo(1);
        await Assert.That(sut.Toasts[0].Type).IsEqualTo(type);
        await Assert.That(sut.Toasts[0].Description).IsNull();
    }

    private sealed class RecordingToastNotifier : IToastNotifier
    {
        public List<Toast> Toasts { get; } = [];

        public void Add(Toast toast) => Toasts.Add(toast);
    }
}
