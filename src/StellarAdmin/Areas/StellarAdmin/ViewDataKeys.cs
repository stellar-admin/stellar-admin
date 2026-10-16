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

    /// <summary>
    ///     The key for the data prepared by a form's field displays, by display field.
    /// </summary>
    public const string DisplayData = "StellarAdminDisplayData";

    /// <summary>
    ///     The key for the form container being rendered.
    /// </summary>
    public const string FormContainer = "StellarAdminFormContainer";

    /// <summary>
    ///     The key for a field's <see cref="FormFieldProperties"/>.
    /// </summary>
    public const string FormFieldProperties = "StellarAdminFormFieldProperties";

    /// <summary>
    ///     Carries the form's <see cref="Options.FormFieldOptions" /> list into the form
    ///     fields partial.
    /// </summary>
    public const string FormFields = "StellarAdminFormFields";

    /// <summary>
    ///     The key for the form's fields and layout containers.
    /// </summary>
    public const string FormItems = "StellarAdminFormItems";

    /// <summary>
    ///     The key for the form section layout.
    /// </summary>
    public const string FormSectionLayout = "StellarAdminFormSectionLayout";

    /// <summary>
    ///     The key for data prepared by form field editors.
    /// </summary>
    public const string EditorData = "StellarAdminEditorData";

    /// <summary>
    ///     The key for templates selected by form field editors.
    /// </summary>
    public const string EditorTemplates = "StellarAdminEditorTemplates";

    // The column definitions of the form scope whose items are rendered
    internal const string FormColumns = "StellarAdminFormColumns";

    // The model state keys whose errors grid cells show, so the row's message shows the others
    internal const string GridCellErrors = "StellarAdminGridCellErrors";

    // The label orientation display fields inherit from their sections
    internal const string FormOrientation = "StellarAdminFormOrientation";

    // The form whose fields a grid row edits ("edit" or "create"), so its lookups open that form's lookup sheet
    internal const string LookupForm = "StellarAdminLookupForm";

    // The stack level of the create sheet a form renders in, so its lookups bind and open the next level
    internal const string SheetLevel = "StellarAdminSheetLevel";
}
