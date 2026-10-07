using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class FixedChoiceItemsProvider : IChoiceItemsProvider
{
    public Task<IReadOnlyList<ChoiceItem>> GetItemsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ChoiceItem>>([new("notebook", "Notebook")]);
}
