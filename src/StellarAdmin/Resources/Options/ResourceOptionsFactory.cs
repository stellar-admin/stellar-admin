using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.UI.Icons;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Options;

// Builds a resource's options without ever throwing. After every Configure and PostConfigure callback, the checks
// repair what they can and report it. Anything that still throws makes the resource unavailable, so one resource's mistake
// can't stop the app or the rest of the admin panel. IOptions caches the result, problems included.
internal sealed class ResourceOptionsFactory<TResource>(
    IEnumerable<IConfigureOptions<ResourceOptions<TResource>>> setups,
    IEnumerable<IPostConfigureOptions<ResourceOptions<TResource>>> postConfigures,
    IEnumerable<IValidateOptions<ResourceOptions<TResource>>> validations,
    IOptions<IconOptions> iconOptions,
    IEnumerable<ResourceRegistration> registrations
) : IOptionsFactory<ResourceOptions<TResource>>
{
    private readonly OptionsFactory<ResourceOptions<TResource>> _factory = new(
        setups,
        postConfigures,
        validations
    );

    public ResourceOptions<TResource> Create(string name)
    {
        var outer = ResourceProblemSink.Open();
        try
        {
            var options = _factory.Create(name);
            options.Problems.InsertRange(0, ResourceProblemSink.Close(outer));
            Check(
                options,
                iconOptions.Value,
                registrations.Select(registration => registration.ResourceType).ToHashSet()
            );

            return options;
        }
        catch (Exception exception)
        {
            ResourceProblemSink.Close(outer);
            var options = new ResourceOptions<TResource>();
            options.Problems.Add(
                ConfigurationProblem.Unavailable(
                    ConfigurationProblemKind.ResourceFailed,
                    $"The `{typeof(TResource).Name.Split('`')[0]}` resource is hidden, and its pages return 404, because its configuration throws `{exception.GetType().Name}`: {exception.Message}",
                    "Fix the configuration code that throws."
                ) with
                {
                    Source = ConfigurationSource.FromException(exception),
                }
            );

            return options;
        }
    }

    // Whole pages and actions first, so their fields aren't checked for nothing; then columns and fields; then
    // what depends on the remaining columns and fields
    internal static void Check(
        ResourceOptions<TResource> options,
        IconOptions icons,
        IReadOnlySet<Type> resourceTypes
    )
    {
        options.RepairLabels();
        ResourceHandlerCheck.Run(options);
        ValueTextCheck.Run(options, resourceTypes);
        BadgeDisplayCheck.Run(options);
        ComputedColumnCheck.Run(options);
        ComputedDisplayFieldCheck.Run(options);
        FieldBinderCheck.Run(options);
        ImageEditorCheck.Run(options);
        IconEditorCheck.Run(options, icons);
        InferredEditorCheck.Run(options, resourceTypes);
        EditorCheck.Run(options);
        CollectionEditorCheck.Run(options);
        DependencyCheck.Run(options);
        GridLayoutCheck.Run(options);
        ResourceHandlerCheck.RunViewFields(options);
        InlineRowCheck.Run(options);
        RowDetailsCheck.Run(options);
        AddAnotherCheck.Run(options);
        ResourceIndexCheck.Run(options);
        options.Problems.AddRange(WidgetsCheck.Run(options.Index.Widgets, null, true));
        if (options.View is { } view)
        {
            options.Problems.AddRange(WidgetsCheck.Run(view.Widgets, null, true));
        }

        CheckFormWidgets(options, options.Create, "create");
        CheckFormWidgets(options, options.Edit, "edit");
    }

    // A form in the index grid has no page, so its widgets have nowhere to show
    private static void CheckFormWidgets(
        ResourceOptions<TResource> options,
        ResourceFormOptions? form,
        string page
    )
    {
        if (form is null)
        {
            return;
        }

        if (
            form
            is ResourceCreateOptions { InlineRow: not null }
                or ResourceEditOptions { InlineRow: not null }
        )
        {
            foreach (var widget in form.Widgets)
            {
                options.Problems.Add(
                    new ConfigurationProblem(
                        ConfigurationProblemKind.IgnoredWidgetSetting,
                        $"The `{widget.Name}` widget isn't shown because the resource's {page} form is in the index grid, which has no {page} page.",
                        $"Remove the {page} page's widgets, or remove `UseInlineRow()`."
                    ) with
                    {
                        Source = widget.Source,
                    }
                );
            }

            form.Widgets.Clear();

            return;
        }

        options.Problems.AddRange(WidgetsCheck.Run(form.Widgets, null, true));
    }
}
