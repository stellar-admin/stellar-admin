using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// A card's title comes from the resource when the card doesn't set one, so the list request loads it; nothing else
// does. A card shows up to four stats, and the title and description sit over the image only when there is one
internal static class CardsLayoutCheck
{
    private const int MaxStats = 4;

    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        foreach (var cards in options.Index.Cards)
        {
            var card = cards.Card;
            card.TitleField ??= options.Title;
            if (card is { Arrangement: ResourceCardArrangement.Overlay, Image: null })
            {
                card.Arrangement = ResourceCardArrangement.Vertical;
                options.Report(
                    ConfigurationProblemKind.CardOverlayWithoutImage,
                    $"The cards of the `{cards.Key}` layout are vertical because they have no image for the title to sit over.",
                    "Add an image with `card.Image(...)`, or use another arrangement.",
                    card.ArrangementSource
                );
            }

            if (card.Stats.Count > MaxStats)
            {
                var extra = card.MutableStats[MaxStats];
                card.MutableStats.RemoveRange(MaxStats, card.MutableStats.Count - MaxStats);
                options.Report(
                    ConfigurationProblemKind.TooManyCardStats,
                    $"The cards of the `{cards.Key}` layout show their first {MaxStats} stats and leave out the rest, starting with `{extra.FieldName ?? extra.Title ?? extra.FieldExpression.ToString()}`.",
                    $"Add at most {MaxStats} stats, and move the others to the fields.",
                    extra.Source
                );
            }
        }
    }
}
