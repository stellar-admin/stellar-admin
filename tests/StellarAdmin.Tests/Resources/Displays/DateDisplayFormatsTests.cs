using System.Globalization;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Tests.Resources.Displays;

public class DateDisplayFormatsTests
{
    [Test]
    [Arguments("en-US", "MMM d, yyyy")]
    [Arguments("en-GB", "d MMM yyyy")]
    [Arguments("de-DE", "d. MMM yyyy")]
    [Arguments("hu-HU", "yyyy. MMM d.")]
    [Arguments("ja-JP", "yyyy年M月d日")]
    public async Task For_Culture_UsesLongDateWithoutWeekday(string culture, string expected)
    {
        // Arrange
        var format = CultureInfo.GetCultureInfo(culture).DateTimeFormat;

        // Act
        var sut = DateDisplayFormats.For(format);

        // Assert
        await Assert.That(sut.Date).IsEqualTo(expected);
    }

    [Test]
    [Arguments("en-US")]
    [Arguments("de-DE")]
    public async Task For_Culture_JoinsDateAndShortTimeWithSpace(string culture)
    {
        // Arrange
        var format = CultureInfo.GetCultureInfo(culture).DateTimeFormat;

        // Act
        var sut = DateDisplayFormats.For(format);

        // Assert
        await Assert.That(sut.Time).IsEqualTo(format.ShortTimePattern);
        await Assert.That(sut.DateAndTime).IsEqualTo($"{sut.Date} {format.ShortTimePattern}");
    }

    [Test]
    public async Task For_InvariantCulture_UsesFixedFormats()
    {
        // Arrange
        var format = CultureInfo.InvariantCulture.DateTimeFormat;

        // Act
        var sut = DateDisplayFormats.For(format);

        // Assert
        await Assert
            .That(sut)
            .IsEqualTo(new DateDisplayFormats("d MMM yyyy", "HH:mm", "d MMM yyyy, HH:mm"));
    }

    [Test]
    public async Task For_FirstLongDateHasWeekday_UsesNextLongDate()
    {
        // Arrange
        var format = LongDates("dddd, d MMMM yyyy", "d MMMM yyyy");

        // Act
        var sut = DateDisplayFormats.For(format);

        // Assert
        await Assert.That(sut.Date).IsEqualTo("d MMM yyyy");
    }

    [Test]
    [Arguments("dddd d MMMM yyyy")]
    [Arguments("ddd d MMMM yyyy")]
    public async Task For_EveryLongDateHasWeekday_UsesFixedFormats(string longDate)
    {
        // Arrange
        var format = LongDates(longDate);

        // Act
        var sut = DateDisplayFormats.For(format);

        // Assert
        await Assert.That(sut.DateAndTime).IsEqualTo("d MMM yyyy, HH:mm");
    }

    [Test]
    [Arguments("'dddd MMMM' d MMMM yyyy", "'dddd MMMM' d MMM yyyy")]
    [Arguments("\"dddd MMMM\" d MMMM yyyy", "\"dddd MMMM\" d MMM yyyy")]
    [Arguments(@"\d\d\d d MMMM yyyy", @"\d\d\d d MMM yyyy")]
    public async Task For_LiteralText_KeepsLiteralText(string longDate, string expected)
    {
        // Arrange
        var format = LongDates(longDate);

        // Act
        var sut = DateDisplayFormats.For(format);

        // Assert
        await Assert.That(sut.Date).IsEqualTo(expected);
    }

    private static DateTimeFormatInfo LongDates(params string[] patterns)
    {
        var format = (DateTimeFormatInfo)CultureInfo.InvariantCulture.DateTimeFormat.Clone();
        format.SetAllDateTimePatterns(patterns, 'D');

        return format;
    }
}
