using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// A form model's metadata that names each property by its label in the form, found by its path, so the framework's
// binding and validation messages name a field as the form labels it. A property on a field's path without a label of its
// own is named as FieldLabels names it. Each node copies the framework's metadata for its type or property into a new
// instance, so the framework's cached metadata keeps its own names, and binders and validators cached for it serve this one
// too. Only the properties on a field's path get a node: the others, and what lies below a field, are the framework's
// metadata, so the binder factory still recognises a type that contains itself.
internal sealed class FormMetadata : DefaultModelMetadata
{
    // A form's metadata, built once, so the binder factory caches one binder for it
    private static readonly ConditionalWeakTable<
        IReadOnlyList<FormFieldOptions>,
        ModelMetadata
    > Forms = new();

    private static readonly Func<ModelMetadata, ModelMetadataIdentity> GetIdentity =
        typeof(ModelMetadata)
            .GetProperty("Identity", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetMethod!.CreateDelegate<Func<ModelMetadata, ModelMetadataIdentity>>();

    private readonly ICompositeMetadataDetailsProvider _detailsProvider;
    private readonly DefaultModelMetadata _inner;
    private readonly IReadOnlyDictionary<string, string> _labels;
    private readonly string _path;
    private readonly IModelMetadataProvider _provider;
    private ModelPropertyCollection? _properties;

    private FormMetadata(
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        DefaultModelMetadata inner,
        string path,
        IReadOnlyDictionary<string, string> labels
    )
        : base(
            provider,
            detailsProvider,
            CopyDetails(inner),
            inner.ModelBindingMessageProvider as DefaultModelBindingMessageProvider ?? new()
        )
    {
        _provider = provider;
        _detailsProvider = detailsProvider;
        _inner = inner;
        _path = path;
        _labels = labels;
    }

    public override string? DisplayName =>
        _path.Length == 0 ? base.DisplayName
        : _labels.TryGetValue(_path, out var label) ? label
        : FieldLabels.Default(_inner);

    public override ModelPropertyCollection Properties =>
        _properties ??= new(
            _inner.Properties.Select(property =>
                property is DefaultModelMetadata metadata
                && (_path.Length == 0 ? metadata.PropertyName! : $"{_path}.{metadata.PropertyName}")
                    is var path
                && IsOnFieldPath(path)
                    ? new FormMetadata(_provider, _detailsProvider, metadata, path, _labels)
                    : property
            )
        );

    // The metadata of a form model whose fields have these labels, by path, or the framework's when its metadata isn't the
    // default kind
    public static ModelMetadata Create(
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        Type modelType,
        IReadOnlyDictionary<string, string> labels
    )
    {
        var metadata = provider.GetMetadataForType(modelType);

        return metadata is DefaultModelMetadata defaultMetadata
            ? new FormMetadata(provider, detailsProvider, defaultMetadata, string.Empty, labels)
            : metadata;
    }

    public static ModelMetadata ForForm(
        IReadOnlyList<FormFieldOptions> fields,
        Type modelType,
        IModelMetadataProvider provider,
        IServiceProvider services
    ) =>
        Forms.GetValue(
            fields,
            _ =>
                Create(
                    provider,
                    services.GetRequiredService<ICompositeMetadataDetailsProvider>(),
                    modelType,
                    fields
                        .DistinctBy(field => field.FieldName, StringComparer.Ordinal)
                        .ToDictionary(
                            field => field.FieldName,
                            field => FieldLabels.Get(field, services, provider),
                            StringComparer.Ordinal
                        )
                )
        );

    private static DefaultMetadataDetails CopyDetails(DefaultModelMetadata metadata) =>
        new(GetIdentity(metadata), metadata.Attributes)
        {
            BindingMetadata = metadata.BindingMetadata,
            BoundConstructorInvoker = metadata.BoundConstructorInvoker,
            BoundConstructorParameters = metadata.BoundConstructorParameters?.ToArray(),
            ContainerMetadata = metadata.ContainerMetadata as DefaultModelMetadata,
            DisplayMetadata = metadata.DisplayMetadata,
            PropertyGetter = metadata.PropertyGetter,
            PropertySetter = metadata.PropertySetter,
            ValidationMetadata = metadata.ValidationMetadata,
        };

    // Whether the path is a field's or leads to one, such as Billing for Billing.Street
    private bool IsOnFieldPath(string path) =>
        _labels.Keys.Any(field =>
            field.StartsWith(path, StringComparison.Ordinal)
            && (field.Length == path.Length || field[path.Length] == '.')
        );

    // Gives the form model's metadata in place of its type's, for binding the form
    public sealed class Provider(IModelMetadataProvider inner, ModelMetadata root)
        : IModelMetadataProvider
    {
        public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType) =>
            inner.GetMetadataForProperties(modelType);

        public ModelMetadata GetMetadataForType(Type modelType) =>
            modelType == root.ModelType ? root : inner.GetMetadataForType(modelType);
    }

    // Validates the form model with its form's metadata
    public sealed class Validator(ObjectModelValidator inner, ModelMetadata root)
        : IObjectModelValidator
    {
        public void Validate(
            ActionContext actionContext,
            ValidationStateDictionary? validationState,
            string prefix,
            object? model
        ) => inner.Validate(actionContext, validationState, prefix, model, root);
    }
}
