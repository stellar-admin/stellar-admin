using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.TagHelpers;

internal sealed class CheckboxGroupValueProviderFactory : IValueProviderFactory
{
    public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        var request = context.ActionContext.HttpContext.Request;
        if (!request.HasFormContentType)
        {
            return;
        }

        var form = await request.ReadFormAsync(context.ActionContext.HttpContext.RequestAborted);
        var names = form.Where(pair =>
                pair.Key.StartsWith(
                    CheckboxGroupModelBinderProvider.MarkerPrefix,
                    StringComparison.Ordinal
                )
                && pair.Value == "true"
            )
            .Select(pair => pair.Key[CheckboxGroupModelBinderProvider.MarkerPrefix.Length..])
            .ToArray();
        if (names.Length > 0)
        {
            context.ValueProviders.Add(new CheckboxGroupPresenceValueProvider(names));
        }
    }
}

internal sealed class CheckboxGroupPresenceValueProvider(string[] names)
    : BindingSourceValueProvider(BindingSource.Form)
{
    private readonly PrefixContainer _prefixes = new(names);

    public override bool ContainsPrefix(string prefix) => _prefixes.ContainsPrefix(prefix);

    public override ValueProviderResult GetValue(string key) => ValueProviderResult.None;
}
