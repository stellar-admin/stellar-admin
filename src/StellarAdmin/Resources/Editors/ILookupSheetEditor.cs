using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Editors;

// What the lookup sheet needs from a single- or multi-select lookup editor
internal interface ILookupSheetEditor
{
    LookupItems? Items { get; }

    string? MediaClassName { get; }

    ReferencedResource? Resource { get; }

    LookupSheetOptions SheetOptions { get; }

    // The items UseItems set, or those of the resource UseResource refers to, found when the form renders
    LookupItems? ResolveItems(IServiceProvider services) =>
        Items
        ?? (
            Resource is { } resource
                ? services.GetRequiredService<ResourceReferences>().GetItems(resource)
                : null
        );
}
