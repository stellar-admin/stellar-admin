using Microsoft.Extensions.Logging;

namespace StellarAdmin.EntityFrameworkCore;

// The log events of the configuration problems EF Core resources report. StellarAdmin reserves 1080 to 1089 for them.
internal static class EfCoreConfigurationEvents
{
    public static readonly EventId EntityNotMapped = new(1080, nameof(EntityNotMapped));
    public static readonly EventId UnsupportedPrimaryKey = new(1081, nameof(UnsupportedPrimaryKey));
    public static readonly EventId UnsupportedManyToManyField = new(
        1082,
        nameof(UnsupportedManyToManyField)
    );
    public static readonly EventId SingleValueManyToManyEditor = new(
        1083,
        nameof(SingleValueManyToManyEditor)
    );
    public static readonly EventId SortableCollectionColumn = new(
        1084,
        nameof(SortableCollectionColumn)
    );
    public static readonly EventId UnsupportedFormField = new(1085, nameof(UnsupportedFormField));
    public static readonly EventId UntranslatableTitle = new(1086, nameof(UntranslatableTitle));
    public static readonly EventId SortableNavigationColumn = new(
        1087,
        nameof(SortableNavigationColumn)
    );
}
