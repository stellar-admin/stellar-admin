using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Each page and action needs a handler, from an explicit delegate or the data source. Without one, the page or action is removed.
internal static class ResourceHandlerCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (
            options.Create is { } create
            && options.CreateHandler is null
            && !Implements<TResource>(options, typeof(IResourceCreateHandler<TResource>))
        )
        {
            options.Create = null;
            options.Report(
                ConfigurationProblemKind.CreateWithoutHandler,
                $"{options.PluralLabel} can't be created because {DataSource(options)} can't create them. There's no New button, and the create page returns 404.",
                ImplementFix(options, "IResourceCreateHandler"),
                create.Source
            );
        }

        if (
            options.Edit is { } edit
            && options.EditHandler is null
            && !Implements<TResource>(options, typeof(IResourceEditHandler<TResource>))
        )
        {
            options.Edit = null;
            options.Report(
                ConfigurationProblemKind.EditWithoutHandler,
                $"{options.PluralLabel} can't be edited because {DataSource(options)} can't edit them. There are no Edit links, and the edit page returns 404.",
                ImplementFix(options, "IResourceEditHandler"),
                edit.Source
            );
        }

        if (
            options.View is { } view
            && options.ViewLoader is null
            && !Implements<TResource>(options, typeof(IResourceViewHandler<TResource>))
        )
        {
            options.View = null;
            options.Report(
                ConfigurationProblemKind.ViewWithoutHandler,
                $"{options.PluralLabel} can't be viewed because {DataSource(options)} can't load one. There are no View links, and the view page returns 404.",
                ImplementFix(options, "IResourceViewHandler"),
                view.Source
            );
        }

        if (
            options.Delete is { } delete
            && !Implements<TResource>(options, typeof(IResourceDeleteHandler<TResource>))
        )
        {
            options.Delete = null;
            options.Report(
                ConfigurationProblemKind.DeleteWithoutHandler,
                $"{options.PluralLabel} can't be deleted because {DataSource(options)} can't delete them. There are no Delete actions.",
                ImplementFix(options, "IResourceDeleteHandler"),
                delete.Source
            );
        }
    }

    // Runs after the field checks, which can leave a view page without fields
    public static void RunViewFields<TResource>(ResourceOptions<TResource> options)
    {
        if (options.View is { Fields.Count: 0 } view)
        {
            options.View = null;
            options.Report(
                ConfigurationProblemKind.ViewWithoutFields,
                "The view page isn't shown because it has no fields. There are no View links, and the view page returns 404.",
                "Add a field to the view page, or remove the view page.",
                view.Source
            );
        }
    }

    // The data source's type, or a description when the resource has none
    private static string DataSource<TResource>(ResourceOptions<TResource> options) =>
        options.DataSourceType is { } type ? $"`{type.Name}`" : "the resource's data source";

    private static string ImplementFix<TResource>(ResourceOptions<TResource> options, string handler) =>
        options.DataSourceType is { } type
            ? $"Implement `{handler}` on `{type.Name}`."
            : $"Use a data source that implements `{handler}`.";

    private static bool Implements<TResource>(ResourceOptions<TResource> options, Type handler) =>
        options.DataSourceType is { } type && handler.IsAssignableFrom(type);
}
