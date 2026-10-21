using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

// Gets the reserved tags from the validation context's services, so it only rejects tags when they're available
public sealed class NotReservedTagsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext
    ) =>
        validationContext.GetService(typeof(ReservedTags)) is ReservedTags reserved
        && value is IEnumerable<Tag> tags
        && tags.Any(tag => reserved.Ids.Contains(tag.Id))
            ? new ValidationResult($"{validationContext.DisplayName} can't include a reserved tag.")
            : ValidationResult.Success;
}
