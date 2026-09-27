using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DashboardPlayground.Resources.Users;

internal static class UserLookups
{
    internal static IReadOnlyList<SelectListItem> Languages() =>
        [
            new("Not specified", ""),
            .. CultureInfo
                .GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
                .Where(culture => !string.IsNullOrEmpty(culture.Name))
                .OrderBy(culture => culture.EnglishName)
                .ThenBy(culture => culture.Name)
                .Select(culture => new SelectListItem(culture.EnglishName, culture.Name)),
        ];

    internal static IReadOnlyList<SelectListItem> TimeZones() =>
        [
            new("Not specified", ""),
            .. TimeZoneInfo
                .GetSystemTimeZones()
                .Select(timeZone => new SelectListItem(
                    $"{timeZone.DisplayName} ({timeZone.Id})",
                    timeZone.Id
                )),
        ];
}
