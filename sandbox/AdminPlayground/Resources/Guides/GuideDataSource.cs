using System.Globalization;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Guides;

// Kept in memory for the lifetime of the app
public sealed class GuideDataSource : IResourceCrudDataSource<Guide>
{
    private readonly List<Guide> _guides =
    [
        new()
        {
            Id = 1,
            Name = "Amara Okafor",
            Bio =
                "<h2>Meet Amara</h2><p>Amara has led <strong>small-group walking tours</strong> through Cape Town for twelve years. She grew up in Bo-Kaap and knows every <em>spice merchant</em> by name.</p><ul><li><p>Table Mountain sunrise hikes</p></li><li><p>Bo-Kaap food walks</p></li></ul>",
        },
        new()
        {
            Id = 2,
            Name = "Kenji Watanabe",
            Bio =
                "<p>Kenji guides tea ceremonies and temple walks in Kyoto.</p><table><tbody><tr><th><p>Tour</p></th><th><p>Length</p></th></tr><tr><td><p>Gion at dusk</p></td><td><p>2 hours</p></td></tr><tr><td><p>Fushimi Inari at dawn</p></td><td><p>3 hours</p></td></tr></tbody></table>",
        },
        new()
        {
            Id = 3,
            Name = "Sofia Almeida",
            Bio =
                "Sofia runs the Lisbon tram tours.\n\nThis bio was written before the field became rich text.",
        },
    ];
    private readonly Lock _lock = new();
    private int _nextId = 4;

    public Task<ResourceOperationResult> CreateAsync(
        Guide model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            model.Id = _nextId++;
            _guides.Add(Copy(model));

            return Task.FromResult(
                ResourceOperationResult.Success(model.Id.ToString(CultureInfo.InvariantCulture))
            );
        }
    }

    public Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key) && _guides.RemoveAll(guide => guide.Id == key) > 0
                    ? ResourceOperationResult.Success()
                    : ResourceOperationResult.NotFound()
            );
        }
    }

    public Task<Guide?> FindAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                    ? _guides.Where(guide => guide.Id == key).Select(Copy).FirstOrDefault()
                    : null
            );
        }
    }

    public Task<ResourceListResult<Guide>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            var items = _guides.OrderBy(guide => guide.Name).Select(Copy).ToList();

            return Task.FromResult(new ResourceListResult<Guide>(items, items.Count));
        }
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Guide model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            if (
                !int.TryParse(id, out var key)
                || _guides.FindIndex(guide => guide.Id == key) is var index && index < 0
            )
            {
                return Task.FromResult(ResourceOperationResult.NotFound());
            }

            model.Id = key;
            _guides[index] = Copy(model);

            return Task.FromResult(ResourceOperationResult.Success());
        }
    }

    private static Guide Copy(Guide guide) =>
        new()
        {
            Bio = guide.Bio,
            Id = guide.Id,
            Name = guide.Name,
        };
}
