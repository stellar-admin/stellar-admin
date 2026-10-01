using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one one-time code scenario, and its display name describes the scenario.
public sealed class OneTimeCodeGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<OneTimeCodeGallery>
{
    [Display(Name = "string")]
    public string? Code { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Display(Name = "string · [Required] · [StringLength(6, MinimumLength = 6)]")]
    public string? RequiredCode { get; set; }

    [StringLength(4)]
    [Display(Name = "string · [StringLength(4)]")]
    public string? Pin { get; set; }

    [Display(Name = "string · this display name is replaced by the field Title")]
    public string? TitledCode { get; set; }

    [Display(Name = "string · [Display(Description)]", Description = "Shown below the code.")]
    public string? DescribedCode { get; set; }

    [Display(Name = "string · field Description")]
    public string? FieldDescribedCode { get; set; }

    [Editable(false)]
    [Display(Name = "string · [Editable(false)]")]
    public string? ReadOnlyCode { get; set; }

    [Display(Name = "string · Length 8")]
    public string? LongCode { get; set; }

    [Display(Name = "string · ClassNames.Control")]
    public string? StyledCode { get; set; }

    public static OneTimeCodeGallery CreateSample() =>
        new()
        {
            Code = "482913",
            RequiredCode = "105277",
            Pin = "7314",
            TitledCode = "660218",
            DescribedCode = "391840",
            FieldDescribedCode = "927705",
            ReadOnlyCode = "554120",
            LongCode = "30118842",
            StyledCode = "118204",
        };
}
