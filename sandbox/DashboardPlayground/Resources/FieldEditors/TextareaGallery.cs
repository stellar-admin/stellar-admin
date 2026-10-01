using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one text area scenario, and its display name describes the scenario.
public sealed class TextareaGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<TextareaGallery>
{
    [DataType(DataType.MultilineText)]
    [Display(Name = "string · [DataType(MultilineText)]")]
    public string? Notes { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    [Display(Name = "string · [DataType(MultilineText)] · [Required]")]
    public string? RequiredNotes { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(
        Name = "string · [DataType(MultilineText)] · [Display(Description)]",
        Description = "Shown below the text area."
    )]
    public string? DescribedNotes { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "string · this display name is replaced by the field Title")]
    public string? TitledNotes { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "string · field Description")]
    public string? FieldDescribedNotes { get; set; }

    [Editable(false)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "string · [Editable(false)]")]
    public string? ReadOnlyNotes { get; set; }

    [Display(Name = "string · Placeholder")]
    public string? PlaceholderNotes { get; set; }

    [Display(Name = "string · Rows 8")]
    public string? TallNotes { get; set; }

    [Display(Name = "string · ClassNames.Control")]
    public string? StyledNotes { get; set; }

    public static TextareaGallery CreateSample() =>
        new()
        {
            Notes = "Window seat, aisle if the flight is over six hours.",
            RequiredNotes = "Vegetarian meal on both legs.",
            DescribedNotes = "Collect the boarding passes at the hotel desk.",
            TitledNotes = "Late check-in agreed with the hotel.",
            FieldDescribedNotes = "Transfer booked for 18:30.",
            ReadOnlyNotes = "Imported from the original booking.",
            PlaceholderNotes = null,
            TallNotes = "Day 1: Lisbon\nDay 2: Sintra\nDay 3: Porto\nDay 4: Douro valley",
            StyledNotes = "Ask about the lounge pass.",
        };
}
