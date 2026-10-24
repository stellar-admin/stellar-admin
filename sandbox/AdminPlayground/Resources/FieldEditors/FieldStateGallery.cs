using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.FieldEditors;

// Read-only and disabled fields, set on the field and, for read-only, decided for each record. The edit page's sample is
// confirmed; the create page's empty record isn't.
public sealed class FieldStateGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<FieldStateGallery>
{
    // The record's state, which the condition reads

    [Display(Name = "Confirmed")]
    public bool Confirmed { get; set; }

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

    public static FieldStateGallery CreateSample() =>
        new()
        {
            Confirmed = true,
            Reference = "VT-2026-0042",
            AgentNotes = "Prefers aisle seats.",
            LegacyCode = "LIS-OLD-7",
            Price = 1299.5m,
        };
}
