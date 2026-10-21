using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

// The scope building shared by the form and view builders: their fields, sections and groups configure the same
// items, so both pages render through the same layout partials
internal static class FormScopeBuilder
{
    // A container's builder records its items' configuration, which runs against the container once the
    // container is built, so the container keeps its place among its siblings
    public static TBuilder AddContainer<TBuilder>(
        Action<Action<IFormScope>> configure,
        Func<Action<Action<IFormScope>>, TBuilder> createBuilder,
        Func<TBuilder, FormContainerOptions> build
    )
    {
        var configuration = new List<Action<IFormScope>>();
        var builder = createBuilder(configuration.Add);
        configure(scope =>
        {
            var container = build(builder);
            foreach (var configureContainer in configuration)
            {
                configureContainer(container);
            }

            scope.Items.Add(container);
        });

        return builder;
    }

    public static ResourceDisplayFieldBuilder AddDisplayField(
        Action<Action<IFormScope>> configure,
        LambdaExpression field,
        string paramName
    )
    {
        var fieldBuilder = ResourceDisplayFieldBuilder.Create(field, paramName);
        configure(scope => scope.Items.Add(fieldBuilder.Build()));

        return fieldBuilder;
    }

    // A section or details block can show without a heading, so a blank title is reported rather than thrown
    public static void ReportBlankTitle(
        Action<Action<IFormScope>> configure,
        string title,
        string container
    )
    {
        ArgumentNullException.ThrowIfNull(title);
        if (string.IsNullOrWhiteSpace(title))
        {
            configure(_ =>
                ResourceProblemSink.Report(
                    ConfigurationProblemKind.InvalidValue,
                    ConfigurationProblemEffect.Ignored,
                    $"A {container} has a blank title. Give it a title.",
                    $"The {container} is shown without a heading."
                )
            );
        }
    }
}
