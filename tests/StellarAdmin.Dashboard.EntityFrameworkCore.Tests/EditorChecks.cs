using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class EditorChecks
{
    public static async Task Run(HttpClient client)
    {
        var html = await client.GetStringAsync("/test-editors");
        var styledHtml = await client.GetStringAsync("/test-editors?styled=true");
        foreach (var property in typeof(EditorTestModel).GetProperties())
        {
            var customClass = $"test-editor-{property.Name}";
            Require(
                Regex.IsMatch(
                    styledHtml,
                    $"""<div[^>]*class="[^"]*sa-field [^"]*{customClass}(?: |")[^>]*>"""
                ),
                $"{property.Name} applies its class to the outer field wrapper"
            );
            Require(
                !Control(styledHtml, property.Name).Contains(customClass),
                "Root class is not applied to the inner control"
            );
        }

        foreach (var property in typeof(EditorTestModel).GetProperties())
        {
            var expectedTag =
                property.PropertyType == typeof(bool) ? "span"
                : property.PropertyType == typeof(bool?)
                || property.PropertyType.IsEnum
                || Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true
                    ? "div"
                : property.Name == "Multiline" ? "textarea"
                : "input";
            Require(
                Regex.IsMatch(
                    styledHtml,
                    $"""<{expectedTag}\b[^>]*class="[^"]*test-control-{property.Name}(?: |")"""
                ),
                $"{property.Name} puts Control on the visible control"
            );
            Require(
                styledHtml.Contains($"test-label-{property.Name}"),
                $"{property.Name} styles its label"
            );
        }

        var styledInvalid = await client.GetStringAsync("/test-editors?styled=true&invalid=true");
        Require(
            Regex.IsMatch(
                styledInvalid,
                """<div[^>]*class="[^"]*test-error-Decimal[^>]*>.*?Enter a valid number\.""",
                RegexOptions.Singleline
            ),
            "Error classes preserve validation messages"
        );
        Require(
            styledHtml.Contains("test-description-Decimal"),
            "Description class reaches metadata help text"
        );
        Require(
            styledHtml.Contains("test-content-Boolean"),
            "Content class reaches horizontal field content"
        );

        await CheckRadioClasses(client);

        foreach (var property in typeof(EditorTestModel).GetProperties())
        {
            Require(html.Contains($"for=\"{property.Name}\""), $"{property.Name} has a label");
            var tag = Control(html, property.Name);
            Require(tag.Contains("sa-"), $"{property.Name} uses a styled control");
        }

        foreach (
            var name in new[]
            {
                "Byte",
                "SByte",
                "Int16",
                "UInt16",
                "Int32",
                "UInt32",
                "Int64",
                "UInt64",
            }
        )
        {
            var tag = Control(html, name);
            Require(
                tag.Contains("type=\"number\"") && tag.Contains("step=\"1\""),
                $"{name} uses integer input"
            );
        }

        foreach (var name in new[] { "Currency", "Decimal", "Single", "Double", "NullableDecimal" })
        {
            var tag = Control(html, name);
            Require(
                tag.Contains("type=\"number\"") && tag.Contains("step=\"any\""),
                $"{name} supports fractions"
            );
        }

        Require(Control(html, "Decimal").Contains("123.456"), "Decimal precision preserved");
        Require(Control(html, "Currency").Contains("123.456"), "Currency precision preserved");
        Require(
            Control(html, "UInt64").Contains("18446744073709551615"),
            "Large integers preserved"
        );
        Require(
            Control(html, "NullableDecimal").Contains("value=\"\""),
            "Null decimal remains empty"
        );
        Require(Control(html, "Boolean").Contains("checked"), "Boolean renders checked");
        Require(
            Control(html, "ReadOnlyBoolean").Contains("disabled"),
            "Read-only checkbox disabled"
        );
        Require(Control(html, "ReadOnlyEnum").Contains("disabled"), "Read-only enum disabled");
        Require(html.Contains("A fractional amount"), "Metadata description preserved");
        Require(html.Contains("Ready to sell"), "Enum display names rendered");
        Require(
            Regex.IsMatch(html, """<option selected="selected" value="1">Ready to sell</option>"""),
            "Enum selection preserved"
        );
        foreach (var name in new[] { "NullableBoolean", "NullableEnum" })
        {
            var select = Regex
                .Match(
                    html,
                    $"""<select[^>]*name="{name}"[^>]*>(.*?)</select>""",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value;
            Require(
                select.Contains("""<option value="">Not set</option>""")
                    && !select.Contains("selected="),
                $"{name} defaults to empty option"
            );
        }
        Require(Control(html, "DateOnly").Contains("2026-09-09"), "DateOnly formatted for browser");
        Require(Control(html, "TimeOnly").Contains("14:30:15"), "TimeOnly formatted for browser");
        Require(
            Control(html, "DateTime").Contains("2026-09-09T14:30:15"),
            "DateTime formatted for browser"
        );
        Require(
            WebUtility.HtmlDecode(Control(html, "DateTimeOffset")).Contains("+07:00"),
            "Offset preserved explicitly"
        );
        Require(Control(html, "Email").Contains("type=\"email\""), "Email input");
        Require(Control(html, "Url").Contains("type=\"url\""), "URL input");
        Require(!Control(html, "Password").Contains("secret"), "Password is not echoed");

        var invalid = await client.GetStringAsync("/test-editors?invalid=true");
        Require(
            Control(invalid, "Decimal").Contains("bad-number"),
            "Attempted value survives validation failure"
        );
        Require(invalid.Contains("Enter a valid number."), "Validation error rendered");

        foreach (var choice in new[] { "true", "false" })
        {
            var selectedHtml = await client.GetStringAsync($"/test-editors?choice={choice}");
            var select = Regex
                .Match(
                    selectedHtml,
                    """<select[^>]*name="NullableBoolean"[^>]*>(.*?)</select>""",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value;
            Require(
                Regex.IsMatch(select, $"""<option value="{choice}" selected="selected">"""),
                $"Nullable boolean selects {choice}"
            );
        }

        using var response = await client.PostAsync(
            "/test-editors",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Decimal"] = "123.456",
                    ["DateOnly"] = "2026-09-09",
                    ["TimeOnly"] = "14:30:15.1234567",
                    ["DateTime"] = "2026-09-09T14:30:15.1234567",
                    ["DateTimeOffset"] = "2026-09-09T14:30:15.0000000+07:00",
                    ["NullableBoolean"] = "",
                    ["NullableDecimal"] = "",
                    ["NullableEnum"] = "",
                    ["NullableDateOnly"] = "",
                    ["NullableDateTime"] = "",
                    ["NullableTimeOnly"] = "",
                    ["Enum"] = "1",
                    ["UInt64"] = "18446744073709551615",
                }
            )
        );
        Require(response.IsSuccessStatusCode, "Scalar form values bind successfully");

        using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var model = result.RootElement;
        Require(
            model.GetProperty("decimal").GetDecimal() == 123.456m,
            "Decimal binds without rounding"
        );
        Require(model.GetProperty("dateOnly").GetString() == "2026-09-09", "DateOnly binds");
        Require(
            model.GetProperty("timeOnly").GetString() == "14:30:15.1234567",
            "TimeOnly precision binds"
        );
        Require(
            model.GetProperty("dateTimeOffset").GetDateTimeOffset().Offset == TimeSpan.FromHours(7),
            "DateTimeOffset binds without losing offset"
        );
        Require(
            model.GetProperty("uInt64").GetUInt64() == ulong.MaxValue,
            "Unsigned integer binds without losing precision"
        );

        foreach (
            var name in new[]
            {
                "nullableBoolean",
                "nullableDecimal",
                "nullableEnum",
                "nullableDateOnly",
                "nullableDateTime",
                "nullableTimeOnly",
            }
        )
        {
            Require(
                model.GetProperty(name).ValueKind == JsonValueKind.Null,
                $"{name} clears to null"
            );
        }

        Console.WriteLine("All scalar editor rendering and binding checks passed.");
    }

    private static async Task CheckRadioClasses(HttpClient client)
    {
        foreach (var template in new[] { "EnumRadioGroup", "EnumRadioChoiceCards" })
        {
            var html = await client.GetStringAsync($"/test-editors/radio?template={template}");
            var card = template == "EnumRadioChoiceCards";
            foreach (
                var (part, tag) in new[]
                {
                    ("radio-root", "fieldset"),
                    ("radio-options", "div"),
                    ("radio-label", "legend"),
                    ("radio-description", "p"),
                    ("radio-error", "div"),
                    ("option-root", card ? "label" : "div"),
                    ("option-control", "span"),
                    ("option-label", card ? "div" : "label"),
                    ("option-content", "div"),
                    ("option-description", "p"),
                }
            )
            {
                Require(
                    Regex.IsMatch(html, $"""<{tag}\b[^>]*class="[^"]*{part}(?: |")"""),
                    $"{template}: {part} reaches {tag}"
                );
            }

            Require(
                html.Contains("Not set") && html.Contains("Available for purchase."),
                "Nullable choice and enum description retained"
            );
            Require(
                Regex.Matches(html, @"Choose a condition\.").Count == 1,
                "Radio validation rendered once"
            );
            Require(
                Regex.Matches(html, "option-control").Count == 3,
                "Each option gets its own control classes"
            );

            var common = await client.GetStringAsync(
                $"/test-editors/radio?template={template}&typed=false"
            );
            Require(
                common.Contains("radio-root") && common.Contains("radio-options"),
                "Common options work without typed configuration"
            );

            var fallback = await client.GetStringAsync(
                $"/test-editors/radio?template={template}&typed=false&flags=true"
            );
            Require(
                Regex.IsMatch(fallback, """<input[^>]*class="[^"]*radio-options"""),
                "Flags fallback retains common Control class"
            );
        }

        foreach (
            var url in new[]
            {
                "/test-editors/radio?template=Enum",
                "/test-editors/radio?flags=true",
            }
        )
        {
            try
            {
                using var response = await client.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
                Require(
                    response.StatusCode == HttpStatusCode.InternalServerError
                        && body.Contains("RadioEditorOptions"),
                    "Incompatible options report a clear error"
                );
            }
            catch (InvalidOperationException exception)
                when (exception.Message.Contains(
                        "RadioEditorOptions requires a compatible editor template"
                    )
                )
            {
                // TestServer can propagate the rendering error directly.
            }
        }
    }

    private static string Control(string html, string name)
    {
        return Regex
            .Match(html, $"""<(?:input|select|textarea)\b[^>]*\bname="{name}"[^>]*>""")
            .Value;
    }

    private static void Require(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException(description);
        }
    }
}
