namespace StellarAdmin.Diagnostics;

// The kind of a configuration problem. Its value is the problem's log event ID, so values never change.
internal enum ConfigurationProblemKind
{
    // The resource's options could not be built
    ResourceFailed = 1000,

    InvalidSlug = 1001,
    SlugInUse = 1002,
    ResourceSlugRepeated = 1003,
    BlankLabel = 1004,

    InvalidPageSizes = 1010,
    InvalidDefaultPageSize = 1011,
    InvalidScope = 1012,
    RepeatedScope = 1013,
    UnknownDefaultScope = 1014,
    UnsortableDefaultSort = 1015,

    CreateWithoutHandler = 1020,
    EditWithoutHandler = 1021,
    ViewWithoutFields = 1022,
    ViewWithoutHandler = 1023,
    DeleteWithoutHandler = 1024,

    InlineRowsWithoutKey = 1030,
    InlineCreateMissingRequired = 1031,
    InlineRowFieldWithoutColumn = 1032,
    UnknownKeptField = 1033,

    ImageFieldWithoutStore = 1040,
    InvalidUploadLimits = 1041,
    EmptyIconList = 1042,
    InlineIconsWithoutList = 1043,
    UnknownIcons = 1044,
    FieldWithoutSetter = 1045,
    MismatchedBadgeText = 1046,
    ValueWithoutText = 1047,
    ComputedColumnWithoutTitle = 1048,
    ComputedColumnSortedByName = 1049,
    RepeatedSortName = 1050,
    ComputedFieldWithoutTitle = 1051,
    MismatchedHandler = 1052,
    ColumnDisplayWithHandler = 1053,
    InvalidSelector = 1054,
    InvalidValue = 1055,
    StaleModelBuilder = 1056,

    UnsuitableEditor = 1057,
    TemplateMismatch = 1058,
    LookupCreateUnavailable = 1059,

    RowDetailsWithoutPanels = 1060,
    RowDetailPanelWithoutName = 1061,
    RepeatedRowDetailPanel = 1062,
    RowDetailsWithoutKey = 1063,

    WordmarkWithSubtitle = 1070,
    InvalidLink = 1071,

    // Reported by an integration through ResourceOptions.ReportProblem, with its own event ID. StellarAdmin's EF Core
    // integration uses 1080 to 1089.
    Integration = 1079,

    // Logged by StellarAdmin.UI's icon options
    SkippedIconMapping = 1090,
}
