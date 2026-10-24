using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace StellarAdmin.UI;

/// <summary>
///     Binds checkbox groups with no selected items, and list and dictionary editors with no items, to
///     empty collections.
/// </summary>
/// <remarks>
///     Browsers submit only checked checkboxes, and an editor with no rows submits no inputs. MVC cannot
///     distinguish such an empty field from a field that was not submitted, so an initialized collection can retain
///     its previous values. Checkbox groups, multiple-select toggle groups and list and dictionary editors render a hidden presence marker to distinguish these cases.
///     When the marker is submitted without values (neither the name nor indexed names such as <c>Name[0]</c>), this binder clears the collection and
///     records the empty submission in ModelState. Otherwise, it delegates to MVC's standard collection or dictionary binder.
///     The accompanying value provider allows MVC to discover nested properties when only the marker is submitted.
///     AddStellarAdminUI registers both providers, placing this provider before MVC's built-in collection binders.
/// </remarks>
internal sealed class CheckboxGroupModelBinderProvider : IModelBinderProvider
{
    internal const string MarkerPrefix = "__sa_checkbox_group.";

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (
            context.BindingInfo.BindingSource is { } source
            && !source.CanAcceptDataFrom(BindingSource.Form)
        )
        {
            return null;
        }

        if (
            CollectionEditorValue.DictionaryTypes(context.Metadata.ModelType)
            is var (keyType, valueType)
        )
        {
            var dictionaryBinder = new DictionaryModelBinderProvider().GetBinder(context);
            return dictionaryBinder == null
                ? null
                : new CheckboxGroupModelBinder(
                    dictionaryBinder,
                    () =>
                        Activator.CreateInstance(
                            typeof(Dictionary<,>).MakeGenericType(keyType, valueType)
                        )!
                );
        }

        var elementType = ChoiceGroupValue.ElementType(context.Metadata.ModelType);
        if (
            elementType == null
            || !ChoiceGroupValue.IsSupported(elementType)
            || Nullable.GetUnderlyingType(elementType) != null
        )
        {
            return null;
        }

        // Preserve MVC's collection binding for submissions containing selected values.
        var isArray = context.Metadata.ModelType.IsArray;
        var binder = isArray
            ? new ArrayModelBinderProvider().GetBinder(context)
            : new CollectionModelBinderProvider().GetBinder(context);
        return binder == null
            ? null
            : new CheckboxGroupModelBinder(
                binder,
                () =>
                    isArray
                        ? Array.CreateInstance(elementType, 0)
                        : Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!
            );
    }

    private sealed class CheckboxGroupModelBinder(IModelBinder inner, Func<object> createEmpty)
        : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;
            if (
                request.HasFormContentType
                && bindingContext.ValueProvider.GetValue(bindingContext.ModelName)
                    == ValueProviderResult.None
            )
            {
                var form = await request.ReadFormAsync(bindingContext.HttpContext.RequestAborted);
                // Unchecked checkboxes and an editor without rows submit no value; the marker
                // distinguishes an empty field from a field that was not submitted.
                var indexed = bindingContext.ModelName + "[";
                if (
                    form[MarkerPrefix + bindingContext.ModelName] == "true"
                    && !form.ContainsKey(bindingContext.ModelName)
                    && !form.Keys.Any(key => key.StartsWith(indexed, StringComparison.Ordinal))
                )
                {
                    // Record the empty submission so rendering does not restore previous values.
                    bindingContext.ModelState.SetModelValue(
                        bindingContext.ModelName,
                        Array.Empty<string>(),
                        string.Empty
                    );
                    bindingContext.Result = ModelBindingResult.Success(createEmpty());
                    return;
                }
            }

            await inner.BindModelAsync(bindingContext);
        }
    }
}
