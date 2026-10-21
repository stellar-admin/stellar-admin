using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Each page and action needs a handler, from an explicit delegate or the data source. Without one, the page or action is removed.
internal static class ResourceHandlerCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (
            options.Create is not null
            && options.CreateHandler is null
            && !Implements<TResource>(options, typeof(IResourceCreateHandler<TResource>))
        )
        {
            options.Create = null;
            options.Report(
                ConfigurationProblemKind.CreateWithoutHandler,
                "The create form requires a data source implementing IResourceCreateHandler or an explicit create handler.",
                "There's no New button, and the create page returns 404."
            );
        }

        if (
            options.Edit is not null
            && options.EditHandler is null
            && !Implements<TResource>(options, typeof(IResourceEditHandler<TResource>))
        )
        {
            options.Edit = null;
            options.Report(
                ConfigurationProblemKind.EditWithoutHandler,
                "The edit form requires a data source implementing IResourceEditHandler or an explicit edit handler.",
                "There are no Edit links, and the edit page returns 404."
            );
        }

        if (
            options.View is not null
            && options.ViewLoader is null
            && !Implements<TResource>(options, typeof(IResourceViewHandler<TResource>))
        )
        {
            options.View = null;
            options.Report(
                ConfigurationProblemKind.ViewWithoutHandler,
                "The view page requires a data source implementing IResourceViewHandler or an explicit view handler.",
                "There are no View links, and the view page returns 404."
            );
        }

        if (
            options.Delete is not null
            && !Implements<TResource>(options, typeof(IResourceDeleteHandler<TResource>))
        )
        {
            options.Delete = null;
            options.Report(
                ConfigurationProblemKind.DeleteWithoutHandler,
                "Delete configuration requires a data source implementing IResourceDeleteHandler.",
                "There are no Delete actions."
            );
        }
    }

    // Runs after the field checks, which can leave a view page without fields
    public static void RunViewFields<TResource>(ResourceOptions<TResource> options)
    {
        if (options.View is { Fields.Count: 0 })
        {
            options.View = null;
            options.Report(
                ConfigurationProblemKind.ViewWithoutFields,
                "The view page requires at least one field.",
                "There are no View links, and the view page returns 404."
            );
        }
    }

    private static bool Implements<TResource>(ResourceOptions<TResource> options, Type handler) =>
        options.DataSourceType is { } type && handler.IsAssignableFrom(type);
}
