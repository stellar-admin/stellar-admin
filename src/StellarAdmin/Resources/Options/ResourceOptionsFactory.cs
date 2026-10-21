using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Resources.Options;

// Builds a resource's options without ever throwing. After every Configure and PostConfigure callback, the checks
// repair what they can and report it. Anything that still throws turns the resource off, so one resource's mistake
// can't stop the app or the rest of the admin panel. IOptions caches the result, problems included.
internal sealed class ResourceOptionsFactory<TResource>(
    IEnumerable<IConfigureOptions<ResourceOptions<TResource>>> setups,
    IEnumerable<IPostConfigureOptions<ResourceOptions<TResource>>> postConfigures,
    IEnumerable<IValidateOptions<ResourceOptions<TResource>>> validations,
    IOptions<IconOptions> iconOptions
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
            Check(options, iconOptions.Value);

            return options;
        }
        catch (Exception exception)
        {
            ResourceProblemSink.Close(outer);
            var options = new ResourceOptions<TResource>();
            options.Report(
                ConfigurationProblemKind.ResourceFailed,
                ConfigurationProblemEffect.ResourceOff,
                $"Configuring the resource failed with {exception.GetType().Name}: {exception.Message}",
                "The resource is turned off: it leaves the sidebar and its pages return 404."
            );

            return options;
        }
    }

    // Whole pages and actions first, so their fields aren't checked for nothing; then columns and fields; then
    // what depends on the remaining columns and fields
    internal static void Check(ResourceOptions<TResource> options, IconOptions icons)
    {
        options.RepairLabels();
        ResourceHandlerCheck.Run(options);
        ValueTextCheck.Run(options);
        BadgeDisplayCheck.Run(options);
        ComputedColumnCheck.Run(options);
        ComputedDisplayFieldCheck.Run(options);
        FieldBinderCheck.Run(options);
        ImageEditorCheck.Run(options);
        IconEditorCheck.Run(options, icons);
        EditorCheck.Run(options);
        GridLayoutCheck.Run(options);
        ResourceHandlerCheck.RunViewFields(options);
        InlineRowCheck.Run(options);
        RowDetailsCheck.Run(options);
        AddAnotherCheck.Run(options);
        ResourceIndexCheck.Run(options);
    }
}
