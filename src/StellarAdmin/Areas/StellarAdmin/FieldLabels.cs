using System.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// A field's label: its title, then the property's [Display(Name)], then for a single value the label of the resource its
// editor takes its items from, so a CategoryId field reads "Category", then the property's name. A collection, such as
// Tags, keeps its property's name, which is already plural.
internal static class FieldLabels
{
    // The label the field sets or takes from its resource, or null when the property's metadata labels it
    public static string? Find(
        FormFieldOptions field,
        IServiceProvider services,
        IModelMetadataProvider metadataProvider
    )
    {
        if (field.Title is { } title)
        {
            return title;
        }

        var resource = field.Editor switch
        {
            ChoiceEditor choice => choice.Resource,
            ILookupSheetEditor lookup => lookup.Resource,
            _ => null,
        };

        var propertyType = field.PropertyPath[^1].PropertyType;
        return
            resource is null
            || propertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyType)
            || GetMetadata(field, metadataProvider).DisplayName is not null
            ? null
            : services.GetRequiredService<ResourceReferences>().GetLabel(resource);
    }

    public static string Get(
        FormFieldOptions field,
        IServiceProvider services,
        IModelMetadataProvider metadataProvider
    ) =>
        Find(field, services, metadataProvider)
        ?? GetMetadata(field, metadataProvider).GetDisplayName();

    private static ModelMetadata GetMetadata(
        FormFieldOptions field,
        IModelMetadataProvider metadataProvider
    )
    {
        var property = field.PropertyPath[^1];

        return metadataProvider.GetMetadataForProperty(property.DeclaringType!, property.Name);
    }
}
