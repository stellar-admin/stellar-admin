using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Editors;

// Where an editor's choices come from: a choice editor's, or a dictionary editor's key or value. The public UseItems and
// UseResource methods of each forward here.
internal sealed class ChoiceSource
{
    // Loads the choices with the form the field is on, which holds only the field's parents when the field depends on
    // other fields
    public Func<
        object,
        IServiceProvider,
        CancellationToken,
        Task<IReadOnlyList<ChoiceItem>>
    >? ItemsLoader
    {
        get;
        private set
        {
            field = value;
            ItemsValueType = null;
            Resource = null;
        }
    }

    // The type of the items' values when the items know it, checked against the field's property
    public Type? ItemsValueType { get; private set; }

    // The resource the items come from, or null when the editor has its own
    public ReferencedResource? Resource { get; private set; }

    public void UseFormItems<TModel>(
        Func<
            TModel,
            IServiceProvider,
            CancellationToken,
            Task<IReadOnlyList<ChoiceItem>>
        > itemsLoader,
        string editorName
    )
    {
        ArgumentNullException.ThrowIfNull(itemsLoader);

        ItemsLoader = (model, services, cancellationToken) =>
            itemsLoader(FormModel.Cast<TModel>(model, editorName), services, cancellationToken);
    }

    public void UseItems<TProvider>()
        where TProvider : class, IChoiceItemsProvider
    {
        ItemsLoader = (_, services, cancellationToken) =>
            services.GetRequiredService<TProvider>().GetItemsAsync(cancellationToken);
    }

    public void UseItems(ChoiceItems items)
    {
        ArgumentNullException.ThrowIfNull(items);

        ItemsLoader = items.LoadAsync;
        ItemsValueType = items.ValueType;
    }

    public void UseItems(IEnumerable<ChoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var snapshot = items.ToArray();
        ItemsLoader = (_, _, _) => Task.FromResult<IReadOnlyList<ChoiceItem>>(snapshot);
    }

    public void UseItems(IEnumerable<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        UseItems(
            items.Select(item => new ChoiceItem(item.Value ?? item.Text, item.Text)
            {
                Disabled = item.Disabled,
                Group = item.Group is null
                    ? null
                    : new ChoiceGroup(item.Group.Name ?? "") { Disabled = item.Group.Disabled },
            })
        );
    }

    public void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>> itemsLoader
    )
    {
        ArgumentNullException.ThrowIfNull(itemsLoader);

        ItemsLoader = (_, services, cancellationToken) => itemsLoader(services, cancellationToken);
    }

    public void UseResource(ReferencedResource resource)
    {
        ItemsLoader = async (model, services, cancellationToken) =>
            services.GetRequiredService<ResourceReferences>().GetItems(resource) is { } items
                ? await items.ListAsync(services, model, cancellationToken)
                : [];
        Resource = resource;
    }
}
