using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StellarAdmin.TagHelpers;

internal sealed class QuestionnaireItemContext
{
    public ModelExpression? For { get; init; }

    public bool Multiple { get; init; }

    public string? Name { get; init; }
}
