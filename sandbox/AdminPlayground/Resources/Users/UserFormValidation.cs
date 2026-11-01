using System.ComponentModel.DataAnnotations;
using AdminPlayground.Data;

namespace AdminPlayground.Resources.Users;

// The rules for a user's fields, which ApplicationUser can't carry as attributes without changing the database model.
// Errors name the fields through the form model's User property, so they show under them
internal static class UserFormValidation
{
    internal static IEnumerable<ValidationResult> Validate(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            yield return new("The Email field is required.", ["User.Email"]);
        }
        else if (!new EmailAddressAttribute().IsValid(user.Email))
        {
            yield return new("The Email field is not a valid e-mail address.", ["User.Email"]);
        }

        foreach (
            var (value, name, title, length) in new[]
            {
                (user.FirstName, nameof(ApplicationUser.FirstName), "First name", 100),
                (user.LastName, nameof(ApplicationUser.LastName), "Last name", 100),
                (
                    user.PreferredLanguage,
                    nameof(ApplicationUser.PreferredLanguage),
                    "Preferred language",
                    35
                ),
                (user.TimeZoneId, nameof(ApplicationUser.TimeZoneId), "Time zone", 35),
            }
        )
        {
            if (value?.Length > length)
            {
                yield return new(
                    $"The {title} field must be at most {length} characters.",
                    [$"User.{name}"]
                );
            }
        }
    }
}
