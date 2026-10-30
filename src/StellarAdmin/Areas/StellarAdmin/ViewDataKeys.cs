namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     Provides the <c>ViewData</c> keys that the admin panel's views use.
/// </summary>
public static class ViewDataKeys
{
    /// <summary>
    ///     Represents the key for the <see cref="Resources.Displays.FieldDisplay" /> of the field
    ///     being rendered.
    /// </summary>
    public const string FieldDisplay = "StellarAdminFieldDisplay";

    /// <summary>
    ///     Represents the key for the data prepared for the display of the field being rendered.
    /// </summary>
    public const string FieldDisplayData = "StellarAdminFieldDisplayData";

    // The fields a refresh renders again when a field they depend on changes, and whether they render in a grid row
    internal const string DependentFields = "StellarAdminDependentFields";

    internal const string DependentFieldsInGrid = "StellarAdminDependentFieldsInGrid";

    internal const string DisplayData = "StellarAdminDisplayData";

    internal const string EditorData = "StellarAdminEditorData";

    internal const string EditorTemplates = "StellarAdminEditorTemplates";

    // The column definitions of the form scope whose items are rendered
    internal const string FormColumns = "StellarAdminFormColumns";

    internal const string FormContainer = "StellarAdminFormContainer";

    internal const string FormFieldProperties = "StellarAdminFormFieldProperties";

    internal const string FormFields = "StellarAdminFormFields";

    internal const string FormItems = "StellarAdminFormItems";

    // The label orientation display fields inherit from their sections
    internal const string FormOrientation = "StellarAdminFormOrientation";

    internal const string FormSectionLayout = "StellarAdminFormSectionLayout";

    // The model state keys whose errors grid cells show, so the row's message shows the others
    internal const string GridCellErrors = "StellarAdminGridCellErrors";

    // The form whose fields a grid row edits ("edit" or "create"), so its lookups open that form's lookup sheet
    internal const string LookupForm = "StellarAdminLookupForm";

    // The widget whose partial view renders, so its forms can request it again
    internal const string Widget = "StellarAdminWidget";

    // The index page's widgets, when its request renders them
    internal const string Widgets = "StellarAdminWidgets";

    // The stack level of the create sheet a form renders in, so its lookups bind and open the next level
    internal const string SheetLevel = "StellarAdminSheetLevel";
}
