using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

// Radios never check a choice for a null model, so the empty choice is selected for a null value until one is posted
internal static class ChoiceSelection
{
    public static bool SelectsEmptyChoice(object? model, ModelStateEntry? state) =>
        model is null && state?.AttemptedValue is null;
}
