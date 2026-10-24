using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

// Read-only and disabled fields, set on the field and decided for each record. The edit page's sample is confirmed and
// has no discount; the create page's empty record is neither.
public sealed class FieldStateGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<FieldStateGallery>
{
    // The record's state, which the conditions read

    [Display(Name = "Confirmed")]
    public bool Confirmed { get; set; }

    [Display(Name = "Has discount")]
    public bool HasDiscount { get; set; }

    // Always

    [Display(Name = "string · ReadOnly()")]
    public string? Reference { get; set; }

    [Display(Name = "string · Disabled()")]
    public string? AgentNotes { get; set; }

    [Display(Name = "string · ReadOnly() · Disabled() is disabled")]
    public string? LegacyCode { get; set; }

    // For each record

    [Display(
        Name = "decimal · ReadOnly(when confirmed)",
        Description = "Read-only on the edit page, whose sample is confirmed."
    )]
    public decimal Price { get; set; }

    [Display(
        Name = "string · Disabled(without a discount)",
        Description = "Disabled on both pages, since neither record has a discount."
    )]
    public string? DiscountCode { get; set; }

    public static FieldStateGallery CreateSample() =>
        new()
        {
            Confirmed = true,
            HasDiscount = false,
            Reference = "VT-2026-0042",
            AgentNotes = "Prefers aisle seats.",
            LegacyCode = "LIS-OLD-7",
            Price = 1299.5m,
            DiscountCode = null,
        };
}
