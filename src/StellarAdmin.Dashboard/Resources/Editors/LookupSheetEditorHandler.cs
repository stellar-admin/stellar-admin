using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Infrastructure.Authorization;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a lookup and resolves the selected item.
/// </summary>
public sealed class LookupSheetEditorHandler(LookupSheetEditor editor, IServiceProvider services)
    : FieldEditorHandler<LookupSheetEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/LookupSheet";

    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var items =
            Editor.Items
            ?? throw new InvalidOperationException(
                $"LookupSheetEditor on {context.FieldName} requires UseItems."
            );

        var item = await items.FindAsync(services, context, cancellationToken);
        var createController = Editor.CreateEnabled
            ? await FindCreateControllerAsync(context, items)
            : null;

        return new LookupSheetEditorData(item, createController);
    }

    // The resource registered for the items' type creates new items; a user it doesn't authorize gets no button
    private async Task<string?> FindCreateControllerAsync(
        FieldEditorContext context,
        LookupItems items
    )
    {
        var type =
            Editor.CreateResourceType
            ?? items.ItemType
            ?? throw new InvalidOperationException(
                $"LookupSheetEditor on {context.FieldName} enables create, but its items have no type. Use EnableCreate<TResource>() to select the resource."
            );
        var resource =
            services
                .GetServices<ResourceRegistration>()
                .FirstOrDefault(registration => registration.ResourceType == type)
            ?? throw new InvalidOperationException(
                $"LookupSheetEditor on {context.FieldName} enables create, but no resource is registered for {type.Name}."
            );
        if (!resource.ResolveCanCreate(services))
        {
            throw new InvalidOperationException(
                $"LookupSheetEditor on {context.FieldName} enables create, but the {resource.ControllerName} resource has no create form."
            );
        }

        var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext!;

        return await AuthorizationMetadata.AuthorizeAsync(
            resource.ResolveAuthorizationMetadata(services),
            httpContext
        )
            ? resource.ControllerName
            : null;
    }
}
