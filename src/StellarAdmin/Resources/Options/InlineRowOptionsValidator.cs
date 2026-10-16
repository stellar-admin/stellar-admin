using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace StellarAdmin.Resources.Options;

internal sealed class InlineRowOptionsValidator<TResource>
    : IValidateOptions<ResourceOptions<TResource>>
{
    public ValidateOptionsResult Validate(string? name, ResourceOptions<TResource> options)
    {
        var failures = new List<string>();
        if (
            options is { Edit.InlineRow: not null } or { Create.InlineRow: not null }
            && options.KeyPropertyName is null
        )
        {
            failures.Add("Inline rows require a resource key. Call UseKey on the resource.");
        }

        if (options.Edit is { InlineRow: not null } edit)
        {
            AddUnmatchedFields(failures, "inline edit row", edit, options.Index.Columns);
        }

        if (options.Create is { InlineRow: not null } create)
        {
            AddUnmatchedFields(failures, "inline create row", create, options.Index.Columns);

            var fields = create
                .Fields.Where(field => !field.IsReadOnly)
                .Select(field => field.FieldName)
                .ToHashSet(StringComparer.Ordinal);
            foreach (
                var property in create.ModelType.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance
                )
            )
            {
                if (
                    property.IsDefined(typeof(RequiredAttribute), inherit: true)
                    && !fields.Contains(property.Name)
                )
                {
                    failures.Add(
                        $"The inline create row has no field for the required property '{property.Name}' of {create.ModelType.Name}. Add the field and a column for it."
                    );
                }
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddUnmatchedFields(
        List<string> failures,
        string row,
        ResourceFormOptions form,
        IEnumerable<DataGridColumnOptions> columns
    )
    {
        var editedFields = columns
            .Select(column => column.EditsFieldName ?? column.FieldName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
        foreach (var field in form.Fields.Where(field => !field.IsReadOnly))
        {
            if (!editedFields.Contains(field.FieldName))
            {
                failures.Add(
                    $"The {row}'s field '{field.FieldName}' matches no index column. Add a column for it, or name it with EditsField on the column that shows it."
                );
            }
        }
    }
}
