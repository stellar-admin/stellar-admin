using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class FixedChoiceItemsProvider : IChoiceItemsProvider
{
    public Task<IReadOnlyList<SelectListItem>> GetItemsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SelectListItem>>([new("Notebook", "notebook")]);
}
