using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one slider scenario, and its display name describes the scenario.
public sealed class SliderGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<SliderGallery>
{
    [Display(Name = "int")]
    public int Volume { get; set; }

    [Range(1, 9)]
    [Display(Name = "int · [Range(1, 9)]")]
    public int Passengers { get; set; }

    [Range(1, 5)]
    [Display(Name = "int? · [Range(1, 5)]")]
    public int? Rating { get; set; }

    [Display(Name = "int · this display name is replaced by the field Title")]
    public int TitledVolume { get; set; }

    [Display(Name = "int · [Display(Description)]", Description = "Shown below the slider.")]
    public int DescribedVolume { get; set; }

    [Display(Name = "int · field Description")]
    public int FieldDescribedVolume { get; set; }

    [Editable(false)]
    [Display(Name = "int · [Editable(false)]")]
    public int ReadOnlyVolume { get; set; }

    [Display(Name = "int · Min 0 · Max 50 · Step 5")]
    public int Discount { get; set; }

    [Display(Name = "int · ClassNames.Control")]
    public int StyledVolume { get; set; }

    public static SliderGallery CreateSample() =>
        new()
        {
            Volume = 40,
            Passengers = 3,
            Rating = null,
            TitledVolume = 60,
            DescribedVolume = 25,
            FieldDescribedVolume = 75,
            ReadOnlyVolume = 50,
            Discount = 15,
            StyledVolume = 30,
        };
}
