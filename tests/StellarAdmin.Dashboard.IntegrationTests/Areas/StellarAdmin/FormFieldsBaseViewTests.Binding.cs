using System.Net;
using System.Text.Json;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin;

public partial class FormFieldsBaseViewTests
{
    [Test]
    [Arguments("nullableBoolean")]
    [Arguments("nullableDecimal")]
    [Arguments("nullableEnum")]
    [Arguments("nullableDateOnly")]
    [Arguments("nullableDateTime")]
    [Arguments("nullableTimeOnly")]
    public async Task Bind_WhenNullableValueIsEmpty_ClearsProperty(string name)
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var response = await app.PostAsync(
            "/test-editors",
            null,
            ("Decimal", "123.456"),
            ("DateOnly", "2026-09-09"),
            ("TimeOnly", "14:30:15.1234567"),
            ("DateTime", "2026-09-09T14:30:15.1234567"),
            ("DateTimeOffset", "2026-09-09T14:30:15.0000000+07:00"),
            ("NullableBoolean", ""),
            ("NullableDecimal", ""),
            ("NullableEnum", ""),
            ("NullableDateOnly", ""),
            ("NullableDateTime", ""),
            ("NullableTimeOnly", ""),
            ("Enum", "1"),
            ("UInt64", "18446744073709551615")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        await Assert
            .That(json.RootElement.GetProperty(name).ValueKind)
            .IsEqualTo(JsonValueKind.Null);
    }

    [Test]
    public async Task Bind_WhenScalarValuesArePosted_PreservesPrecisionAndOffset()
    {
        // Arrange
        await using var app = new TestApplication();

        // Act
        using var response = await app.PostAsync(
            "/test-editors",
            null,
            ("Decimal", "123.456"),
            ("DateOnly", "2026-09-09"),
            ("TimeOnly", "14:30:15.1234567"),
            ("DateTime", "2026-09-09T14:30:15.1234567"),
            ("DateTimeOffset", "2026-09-09T14:30:15.0000000+07:00"),
            ("NullableBoolean", ""),
            ("NullableDecimal", ""),
            ("NullableEnum", ""),
            ("NullableDateOnly", ""),
            ("NullableDateTime", ""),
            ("NullableTimeOnly", ""),
            ("Enum", "1"),
            ("UInt64", "18446744073709551615")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var model = json.RootElement;
        await Assert.That(model.GetProperty("decimal").GetDecimal()).IsEqualTo(123.456m);
        await Assert.That(model.GetProperty("dateOnly").GetString()).IsEqualTo("2026-09-09");
        await Assert.That(model.GetProperty("timeOnly").GetString()).IsEqualTo("14:30:15.1234567");
        await Assert
            .That(model.GetProperty("dateTime").GetString())
            .IsEqualTo("2026-09-09T14:30:15.1234567");
        await Assert
            .That(model.GetProperty("dateTimeOffset").GetDateTimeOffset().Offset)
            .IsEqualTo(TimeSpan.FromHours(7));
        await Assert.That(model.GetProperty("uInt64").GetUInt64()).IsEqualTo(ulong.MaxValue);
    }
}
