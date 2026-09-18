using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Binds checkbox groups with no selected items to empty collections.
/// </summary>
/// <remarks>
///     Browsers submit only checked checkboxes. When every item is unchecked, MVC cannot distinguish
///     an empty selection from a field that was not submitted, so an initialized collection can retain
///     its previous values. Checkbox groups render a hidden presence marker to distinguish these cases.
///     When the marker is submitted without selected values, this binder clears the collection and
///     records the empty submission in ModelState. Otherwise, it delegates to MVC's standard collection binder.
///     The accompanying value provider allows MVC to discover nested properties when only the marker is submitted.
///     AddTagHelpers registers both providers, placing this provider before MVC's built-in collection binders.
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
        var binder = context.Metadata.ModelType.IsArray
            ? new ArrayModelBinderProvider().GetBinder(context)
            : new CollectionModelBinderProvider().GetBinder(context);
        return binder == null ? null : new CheckboxGroupModelBinder(binder, elementType);
    }

    private sealed class CheckboxGroupModelBinder(IModelBinder inner, Type elementType)
        : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;
            if (
                request.HasFormContentType
                && !ContainsSubmittedPrefix(bindingContext.ValueProvider, bindingContext.ModelName)
            )
            {
                var form = await request.ReadFormAsync(bindingContext.HttpContext.RequestAborted);
                // Unchecked checkboxes submit no value; the marker distinguishes an empty group
                // from a field that was not submitted.
                if (form[MarkerPrefix + bindingContext.ModelName] == "true")
                {
                    var empty = bindingContext.ModelType.IsArray
                        ? (object)Array.CreateInstance(elementType, 0)
                        : Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                    // Record the empty submission so rendering does not restore previous selections.
                    bindingContext.ModelState.SetModelValue(
                        bindingContext.ModelName,
                        Array.Empty<string>(),
                        string.Empty
                    );
                    bindingContext.Result = ModelBindingResult.Success(empty);
                    return;
                }
            }

            await inner.BindModelAsync(bindingContext);
        }

        private static bool ContainsSubmittedPrefix(IValueProvider provider, string name)
        {
            return provider switch
            {
                // This provider signals that the group exists, not that any items were selected.
                CheckboxGroupPresenceValueProvider => false,
                CompositeValueProvider composite => composite.Any(child =>
                    ContainsSubmittedPrefix(child, name)
                ),
                _ => provider.ContainsPrefix(name),
            };
        }
    }
}
