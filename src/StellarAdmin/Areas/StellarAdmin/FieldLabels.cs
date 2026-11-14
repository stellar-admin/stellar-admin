using System.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin;

// A field's label: its title, then the property's [Display(Name)], then for a single value the label of the resource its
// editor takes its items from, so a CategoryId field reads "Category", then the property's name split into words, as
// index headers show it. A collection, such as Tags, skips the resource's label for its property's name, which is already
// plural. Forms bind and validate with the same labels (FormMetadata), so their messages name each field as it's labelled.
internal static class FieldLabels
{
    // The label of the field whose display template renders, from its form field or index column, else its property's
    public static string Get(ViewDataDictionary viewData) =>
        viewData[ViewDataKeys.FieldLabel] as string ?? Default(viewData.ModelMetadata);

    public static string Get(FormDisplayFieldOptions field, ModelMetadata metadata) =>
        field.Title ?? Default(metadata);

    public static string Get(
        FormFieldOptions field,
        IServiceProvider services,
        IModelMetadataProvider metadataProvider
    )
    {
        if (field.Title is { } title)
        {
            return title;
        }

        var metadata = GetMetadata(field, metadataProvider);
        var resource = field.Editor switch
        {
            ChoiceEditor choice => choice.Resource,
            LookupSheetEditorBase lookup => lookup.Resource,
            _ => null,
        };
        var propertyType = field.PropertyPath[^1].PropertyType;

        var resourceLabel =
            resource is null
            || propertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyType)
            || metadata.DisplayName is not null
                ? null
                : services.GetRequiredService<ResourceReferences>().GetLabel(resource);

        return resourceLabel ?? Default(metadata);
    }

    // The label of a property without a title. A value that isn't a property, such as a computed field's, keeps the
    // framework's name for it
    public static string Default(ModelMetadata metadata) =>
        metadata.DisplayName
        ?? (
            metadata.PropertyName is { } name
                ? DisplayNames.FromPropertyName(name)
                : metadata.GetDisplayName()
        );

    private static ModelMetadata GetMetadata(
        FormFieldOptions field,
        IModelMetadataProvider metadataProvider
    )
    {
        var property = field.PropertyPath[^1];

        return metadataProvider.GetMetadataForProperty(property.DeclaringType!, property.Name);
    }
}
