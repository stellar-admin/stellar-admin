using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Contains context of a questionnaire item Tag Helper to be shared with it's child Tag Helpers.
/// </summary>
internal sealed class QuestionnaireItemContext
{
    /// <summary>
    ///     Whether a <c>sa-questionnaire-error</c> inside the question has rendered the answer's
    ///     validation message, which stops the item rendering one of its own.
    /// </summary>
    public bool ErrorRendered { get; set; }

    /// <summary>
    ///     The expression the question is bound to, or <c>null</c> when it is unbound. The
    ///     choices take their name and selected state from it, and the error reports it.
    /// </summary>
    public ModelExpression? For { get; init; }

    /// <summary>
    ///     Whether the question accepts more than one answer, which renders its choices as
    ///     checkboxes rather than radio buttons.
    /// </summary>
    public bool Multiple { get; init; }

    /// <summary>
    ///     The name the answer posts under, for a question that is not bound with
    ///     <c>asp-for</c>.
    /// </summary>
    public string? Name { get; init; }
}
