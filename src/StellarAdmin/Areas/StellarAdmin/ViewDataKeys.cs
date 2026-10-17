namespace StellarAdmin.Areas.StellarAdmin;

/// <summary>
///     The <c>ViewData</c> keys the StellarAdmin views communicate through.
/// </summary>
public static class ViewDataKeys
{
    /// <summary>
    ///     The key for the <see cref="Resources.Displays.FieldDisplay" /> of the display field being
    ///     rendered.
    /// </summary>
    public const string FieldDisplay = "StellarAdminFieldDisplay";

    /// <summary>
    ///     The key for the data prepared for the display field being rendered.
    /// </summary>
    public const string FieldDisplayData = "StellarAdminFieldDisplayData";

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

    // The stack level of the create sheet a form renders in, so its lookups bind and open the next level
    internal const string SheetLevel = "StellarAdminSheetLevel";
}
