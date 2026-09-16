namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

/// <summary>
///     The <c>ViewData</c> keys the StellarAdmin views communicate through.
/// </summary>
public static class ViewDataKeys
{
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

    /// <summary>The key for the form's reference choices.</summary>
    public const string ReferenceChoices = "StellarAdminReferenceChoices";
}
