using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    // Sets the kept fields of a new model from the values posted for the previous one. A value that doesn't bind is left
    // out, and the form shows no errors for them
    private async Task BindKeptFieldsAsync(object model, IValueProvider source, string prefix)
    {
        var create = _resourceOptions.Create!;
        var kept = GetEditableFieldNames(create.Fields, model);
        kept.IntersectWith(create.AddAnother!.KeptFields);
        var boundFields = create
            .Fields.Where(field => field.Binder is not null && kept.Contains(field.FieldName))
            .ToArray();
        var fields = kept.Except(boundFields.Select(field => field.FieldName))
            .ToHashSet(StringComparer.Ordinal);
        if (fields.Count > 0)
        {
            var propertyNames = fields
                .SelectMany(field => field.Split('.'))
                .ToHashSet(StringComparer.Ordinal);
            await TryUpdateModelAsync(
                model,
                create.ModelType,
                prefix,
                new ConfiguredFieldValueProvider(source, fields, prefix),
                metadata => propertyNames.Contains(metadata.PropertyName ?? "")
            );
        }

        await BindWithBindersAsync(model, boundFields, source, prefix);
        ModelState.Clear();
    }

    // The posted values stay in the model state, so an editor shows them again when the form has errors
    private async Task BindWithBindersAsync(
        object model,
        IEnumerable<FormFieldOptions> fields,
        IValueProvider source,
        string prefix
    )
    {
        foreach (var field in fields)
        {
            var key = ModelNames.CreatePropertyModelName(prefix, field.FieldName);
            var values = source.GetValue(key).Values;
            ModelState.SetModelValue(key, values, values.ToString());

            var context = new FieldBindingContext(
                field,
                model,
                values,
                HttpContext.RequestServices
            );
            await field.Binder!.BindAsync(context, HttpContext.RequestAborted);
            var errors =
                context.Errors.Count > 0 ? context.Errors : ValidateBoundField(field, model);
            foreach (var error in errors)
            {
                ModelState.AddModelError(key, error);
            }

            if (errors.Count == 0)
            {
                ModelState.MarkFieldValid(key);
            }
        }
    }

    // Binds the values of the fields the dependents depend on, from the values posted or sent with the Model prefix.
    // The dependents' own values aren't bound, since a change of a field they depend on clears them. A value that doesn't
    // bind is left out, and nothing reports it.
    private async Task BindParentsAsync(
        object model,
        ResourceFormOptions form,
        IReadOnlyCollection<FormFieldOptions> dependents
    )
    {
        var dependentNames = dependents
            .Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);
        var parents = dependents
            .SelectMany(field => field.Dependencies)
            .Select(dependency => dependency.Parent)
            .OfType<FormFieldOptions>()
            .Where(parent => !dependentNames.Contains(parent.FieldName))
            .Distinct()
            .ToArray();
        var source = await CompositeValueProvider.CreateAsync(ControllerContext);
        var fields = parents
            .Where(parent => parent.Binder is null)
            .Select(parent => parent.FieldName)
            .ToHashSet(StringComparer.Ordinal);
        if (fields.Count > 0)
        {
            var propertyNames = fields
                .SelectMany(field => field.Split('.'))
                .ToHashSet(StringComparer.Ordinal);
            await TryUpdateModelAsync(
                model,
                form is ResourceEditOptions edit
                    ? edit.ModelType
                    : ((ResourceCreateOptions)form).ModelType,
                DependencyPrefix,
                new ConfiguredFieldValueProvider(source, fields, DependencyPrefix),
                metadata => propertyNames.Contains(metadata.PropertyName ?? "")
            );
        }

        await BindWithBindersAsync(
            model,
            parents.Where(parent => parent.Binder is not null),
            source,
            DependencyPrefix
        );
        ModelState.Clear();
    }

    private async Task<bool> TryBindConfiguredFieldsAsync(
        object model,
        Type modelType,
        IReadOnlyList<FormFieldOptions> formFields,
        HashSet<string> fields,
        string prefix
    )
    {
        // A field with a binder is bound by it, after MVC binds the others
        var boundFields = formFields
            .Where(field => field.Binder is not null && fields.Contains(field.FieldName))
            .ToArray();
        fields = fields
            .Except(boundFields.Select(field => field.FieldName), StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var propertyNames = fields
            .SelectMany(field => field.Split('.'))
            .ToHashSet(StringComparer.Ordinal);

        // An icon field's stored value, which saves unchanged even when it isn't an icon the field offers
        var iconFields = formFields
            .Where(field => field is { Editor: IconEditor } && fields.Contains(field.FieldName))
            .Select(field =>
                (field, ResourcePropertyPath.GetValue(model, field.PropertyPath) as string)
            )
            .ToArray();

        var source = await CompositeValueProvider.CreateAsync(ControllerContext);
        if (
            !await TryUpdateFormModelAsync(
                model,
                modelType,
                formFields,
                prefix,
                new ConfiguredFieldValueProvider(source, fields, prefix),
                metadata => propertyNames.Contains(metadata.PropertyName ?? "")
            )
        )
        {
            ScopeValidationErrors(
                fields,
                formFields
                    .Where(CollectionEditorValidator.ShowsRowErrors)
                    .Select(field => field.FieldName),
                prefix
            );
        }

        CollectionEditorValidator.Validate(
            ModelState,
            source,
            formFields.Where(field => fields.Contains(field.FieldName)),
            prefix,
            HttpContext.RequestServices,
            MetadataProvider
        );
        ValidateIconFields(model, iconFields, prefix);
        SanitizeRichTextFields(model, formFields, fields);
        await BindWithBindersAsync(model, boundFields, source, prefix);

        return ModelState.IsValid;
    }

    // Binds and validates the form's model with metadata that names each property as the form labels it, so binding
    // and validation messages name a field by its label
    private async Task<bool> TryUpdateFormModelAsync(
        object model,
        Type modelType,
        IReadOnlyList<FormFieldOptions> formFields,
        string prefix,
        IValueProvider valueProvider,
        Func<ModelMetadata, bool> propertyFilter
    )
    {
        var metadataProvider = MetadataProvider;
        var objectValidator = ObjectValidator;
        var root = FormMetadata.ForForm(
            formFields,
            modelType,
            metadataProvider,
            HttpContext.RequestServices
        );
        MetadataProvider = new FormMetadata.Provider(metadataProvider, root);
        if (objectValidator is ObjectModelValidator validator)
        {
            ObjectValidator = new FormMetadata.Validator(validator, root);
        }

        try
        {
            return await TryUpdateModelAsync(
                model,
                modelType,
                prefix,
                valueProvider,
                propertyFilter
            );
        }
        finally
        {
            MetadataProvider = metadataProvider;
            ObjectValidator = objectValidator;
        }
    }

    // Model validation runs before the binders, so the attributes of a bound field's property, such as [MinLength],
    // validate the value it bound
    private IReadOnlyList<string> ValidateBoundField(FormFieldOptions field, object model)
    {
        var property = field.PropertyPath[^1];
        var container = ResourcePropertyPath.GetValue(model, field.PropertyPath.SkipLast(1));
        if (container is null)
        {
            return [];
        }

        var context = new ValidationContext(container, HttpContext.RequestServices, items: null)
        {
            DisplayName = FieldLabels.Get(field, HttpContext.RequestServices, MetadataProvider),
            MemberName = property.Name,
        };
        var value = property.GetValue(container);

        return property
            .GetCustomAttributes<ValidationAttribute>(inherit: true)
            .Select(attribute => attribute.GetValidationResult(value, context)?.ErrorMessage)
            .OfType<string>()
            .ToArray();
    }

    // A new value must be an icon the field offers: one of its list, or any registered icon
    private void ValidateIconFields(
        object model,
        (FormFieldOptions Field, string? Stored)[] iconFields,
        string prefix
    )
    {
        if (iconFields.Length == 0)
        {
            return;
        }

        var icons = HttpContext.RequestServices.GetRequiredService<IOptions<IconOptions>>().Value;
        foreach (var (field, stored) in iconFields)
        {
            var key = ModelNames.CreatePropertyModelName(prefix, field.FieldName);
            if (
                ModelState.GetFieldValidationState(key) == ModelValidationState.Invalid
                || ResourcePropertyPath.GetValue(model, field.PropertyPath)
                    is not string { Length: > 0 } value
                || value == stored
            )
            {
                continue;
            }

            var offered = ((IconEditor)field.Editor!).Icons is { Count: > 0 } list
                ? list.Contains(value, StringComparer.OrdinalIgnoreCase)
                : icons.TryGetIcon(value, out _);
            if (!offered)
            {
                ModelState.AddModelError(key, "Choose an icon from the list.");
            }
        }
    }

    // A rich text field's HTML is sanitized as it's bound, so only safe markup is saved: a field with the rich text
    // editor, or one whose property is [DataType(DataType.Html)] and has no other editor
    private void SanitizeRichTextFields(
        object model,
        IReadOnlyList<FormFieldOptions> formFields,
        HashSet<string> fields
    )
    {
        IRichTextSanitizer? sanitizer = null;
        foreach (var field in formFields)
        {
            var property = field.PropertyPath[^1];
            var richText =
                field.Editor is RichTextEditor
                || field.Editor is null
                    && property.GetCustomAttribute<DataTypeAttribute>()?.DataType == DataType.Html;
            if (
                !richText
                || field.Binder is not null
                || !fields.Contains(field.FieldName)
                || property.PropertyType != typeof(string)
                || ResourcePropertyPath.GetValue(model, field.PropertyPath.SkipLast(1))
                    is not { } container
                || property.GetValue(container) is not string { Length: > 0 } html
            )
            {
                continue;
            }

            sanitizer ??= HttpContext.RequestServices.GetRequiredService<IRichTextSanitizer>();
            property.SetValue(container, sanitizer.Sanitize(html));
        }
    }

    // Validation checks the whole model, but the form only sets its configured fields. The errors of
    // other properties are dropped, such as a required navigation whose foreign key is a field, and
    // the data source still rejects what it can't save. An error no field displays, such as the
    // model's own, moves to the summary so a submission never fails without a message. A list or
    // dictionary editor shows an item's error, such as Highlights[2], under its row.
    private void ScopeValidationErrors(
        HashSet<string> fields,
        IEnumerable<string> rowFields,
        string prefix
    )
    {
        var fieldKeys = fields
            .Select(field => ModelNames.CreatePropertyModelName(prefix, field))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var rowFieldKeys = rowFields
            .Where(fields.Contains)
            .Select(field => ModelNames.CreatePropertyModelName(prefix, field))
            .ToArray();
        var unshown = new List<string>();
        foreach (var (key, entry) in ModelState.ToArray())
        {
            if (
                entry.Errors.Count == 0
                || key.Length == 0
                || fieldKeys.Contains(key)
                || rowFieldKeys.Any(fieldKey =>
                    key.StartsWith(fieldKey + "[", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                continue;
            }

            var withinField = fieldKeys.Any(fieldKey =>
                key.StartsWith(fieldKey + ".", StringComparison.OrdinalIgnoreCase)
                || key.StartsWith(fieldKey + "[", StringComparison.OrdinalIgnoreCase)
            );
            var otherProperty =
                !withinField
                && (
                    prefix.Length == 0
                    || key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)
                );
            if (!otherProperty)
            {
                unshown.AddRange(
                    entry.Errors.Select(error =>
                        string.IsNullOrEmpty(error.ErrorMessage)
                            ? $"The value for {key} is invalid."
                            : error.ErrorMessage
                    )
                );
            }

            entry.Errors.Clear();
            entry.ValidationState = ModelValidationState.Valid;
        }

        foreach (var message in unshown.Distinct())
        {
            ModelState.AddModelError(string.Empty, message);
        }
    }

    private sealed class ConfiguredFieldValueProvider(
        IValueProvider source,
        HashSet<string> fields,
        string bindingPrefix
    ) : IValueProvider
    {
        private readonly HashSet<string> _keys = fields
            .Select(field => ModelNames.CreatePropertyModelName(bindingPrefix, field))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _prefixes = CreatePrefixes(fields, bindingPrefix);

        public bool ContainsPrefix(string prefix) =>
            (_prefixes.Contains(prefix) || IsItem(prefix)) && source.ContainsPrefix(prefix);

        public ValueProviderResult GetValue(string key) =>
            _keys.Contains(key) || IsItem(key) ? source.GetValue(key) : ValueProviderResult.None;

        // An item of a field's collection, such as Highlights[0] or LocalNames[0].Key
        private bool IsItem(string key) =>
            _keys.Any(field => key.StartsWith(field + "[", StringComparison.OrdinalIgnoreCase));

        private static HashSet<string> CreatePrefixes(HashSet<string> fields, string bindingPrefix)
        {
            var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { bindingPrefix };
            foreach (var field in fields)
            {
                var prefix = bindingPrefix;
                foreach (var segment in field.Split('.'))
                {
                    prefix = ModelNames.CreatePropertyModelName(prefix, segment);
                    prefixes.Add(prefix);
                }
            }

            return prefixes;
        }
    }
}
