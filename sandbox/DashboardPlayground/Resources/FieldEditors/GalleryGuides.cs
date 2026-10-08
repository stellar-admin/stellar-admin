using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.FieldEditors;

// The tour guides of the multi-select lookup gallery
public sealed record GalleryGuide(string Id, string Name, string Region, string Languages);

// Searches the guides in memory. A real source would query a database or an API.
public sealed class GalleryGuideLookupSource : ILookupSource<GalleryGuide, string>
{
    private static readonly GalleryGuide[] Guides =
    [
        new("ana-ribeiro", "Ana Ribeiro", "Lisbon and the Algarve", "Portuguese, English"),
        new("bruno-costa", "Bruno Costa", "Porto and the Douro", "Portuguese, Spanish"),
        new("camila-torres", "Camila Torres", "Andalusia", "Spanish, English"),
        new("diego-navarro", "Diego Navarro", "Madrid and Toledo", "Spanish, French"),
        new("elena-marchetti", "Elena Marchetti", "Rome and Lazio", "Italian, English"),
        new("farid-haddad", "Farid Haddad", "Cairo and Luxor", "Arabic, English"),
        new("grace-mwangi", "Grace Mwangi", "Maasai Mara", "Swahili, English"),
        new("hiro-tanaka", "Hiro Tanaka", "Kyoto and Nara", "Japanese, English"),
        new("ingrid-larsen", "Ingrid Larsen", "Iceland's Ring Road", "Icelandic, English"),
        new("jonas-weber", "Jonas Weber", "The Swiss Alps", "German, French"),
        new("kavya-nair", "Kavya Nair", "Kerala", "Malayalam, Hindi, English"),
        new("lucas-moreau", "Lucas Moreau", "Paris and the Loire", "French, English"),
        new("maya-cohen", "Maya Cohen", "New York City", "English, Hebrew"),
        new("nikos-pappas", "Nikos Pappas", "Athens and the Cyclades", "Greek, English"),
        new("olivia-brown", "Olivia Brown", "Sydney and the Blue Mountains", "English"),
        new("pedro-alves", "Pedro Alves", "Rio de Janeiro", "Portuguese, English"),
        new(
            "sipho-dlamini",
            "Sipho Dlamini",
            "Cape Town and the Winelands",
            "Zulu, Afrikaans, English"
        ),
        new("thandi-mokoena", "Thandi Mokoena", "Kruger National Park", "Sotho, English"),
    ];

    public Task<IReadOnlyCollection<GalleryGuide>> FindAsync(
        IReadOnlyCollection<string> values,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<IReadOnlyCollection<GalleryGuide>>(
            Guides.Where(guide => values.Contains(guide.Id)).ToArray()
        );

    public Task<LookupPage<GalleryGuide>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = Guides
            .Where(guide =>
                string.IsNullOrEmpty(query.Term)
                || guide.Name.Contains(query.Term, StringComparison.CurrentCultureIgnoreCase)
                || guide.Region.Contains(query.Term, StringComparison.CurrentCultureIgnoreCase)
            )
            .ToArray();

        return Task.FromResult(
            new LookupPage<GalleryGuide>(
                matches.Skip(query.Skip).Take(query.Take).ToArray(),
                matches.Length > query.Skip + query.Take
            )
        );
    }
}
