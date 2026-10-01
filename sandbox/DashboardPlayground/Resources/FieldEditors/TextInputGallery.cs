using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one text input scenario, and its display name describes the scenario.
public sealed class TextInputGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<TextInputGallery>
{
    // Data-type templates forwarding to the text input

    [Display(Name = "string")]
    public string? PlainText { get; set; }

    [Required]
    [Display(Name = "string · [Required]")]
    public string? RequiredText { get; set; }

    [Display(Name = "string · [Display(Description)]", Description = "Shown below the input.")]
    public string? DescribedText { get; set; }

    [EmailAddress]
    [Display(Name = "string · [EmailAddress]")]
    public string? Email { get; set; }

    [Phone]
    [Display(Name = "string · [Phone]")]
    public string? Phone { get; set; }

    [Url]
    [Display(Name = "string · [Url]")]
    public string? Website { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "string · [DataType(Password)]")]
    public string? Password { get; set; }

    [Display(Name = "Guid")]
    public Guid ExternalId { get; set; }

    [Display(Name = "byte")]
    public byte Level { get; set; }

    [Display(Name = "int")]
    public int Quantity { get; set; }

    [Display(Name = "int?")]
    public int? OptionalQuantity { get; set; }

    [Display(Name = "long")]
    public long Population { get; set; }

    [Display(Name = "double")]
    public double Ratio { get; set; }

    [Display(Name = "float")]
    public float Weight { get; set; }

    [Range(typeof(decimal), "0.01", "1000")]
    [Display(Name = "decimal · [Range(0.01, 1000)]")]
    public decimal Price { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "decimal · [DataType(Currency)]")]
    public decimal Budget { get; set; }

    // Field configuration

    [Display(Name = "string · this display name is replaced by the field Title")]
    public string? TitledText { get; set; }

    [Display(Name = "string · field Description")]
    public string? FieldDescribedText { get; set; }

    [Display(
        Name = "string · [Display(Description)] replaced by field Description · Prefix @",
        Description = "This attribute description should not appear."
    )]
    public string? OverriddenDescription { get; set; }

    [Editable(false)]
    [Display(Name = "string · [Editable(false)]")]
    public string? ReadOnlyText { get; set; }

    [Editable(false)]
    [Display(Name = "decimal · [Editable(false)] · Prefix $")]
    public decimal ReadOnlyAmount { get; set; }

    // UseEditor<TextInputEditor> settings

    [Display(Name = "string · Placeholder")]
    public string? PlaceholderText { get; set; }

    [Display(Name = "string · Type = Email")]
    public string? ExplicitEmail { get; set; }

    [Display(Name = "int · Type = Text")]
    public int TextQuantity { get; set; }

    [Display(Name = "decimal · Min 0 · Max 500 · Step 0.25")]
    public decimal Constrained { get; set; }

    [Display(Name = "string · Prefix https://")]
    public string? Domain { get; set; }

    [Display(Name = "decimal · Suffix kg")]
    public decimal Mass { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "1000")]
    [Display(
        Name = "decimal · [Required] · [Range] · [Display(Description)] · Prefix $ · Suffix USD",
        Description = "The input group composes its own label, description and error."
    )]
    public decimal? GroupedPrice { get; set; }

    [StringLength(6, MinimumLength = 6)]
    [Display(Name = "string · [StringLength(6, 6)] · Placeholder · Prefix # · ClassNames.Control")]
    public string? StyledCode { get; set; }

    public static TextInputGallery CreateSample() =>
        new()
        {
            PlainText = "Voyager Travel",
            RequiredText = "Lisbon getaway",
            DescribedText = "Seven nights",
            Email = "bookings@voyager.example",
            Phone = "+1 555 0100",
            Website = "https://voyager.example",
            Password = "secret",
            ExternalId = Guid.Parse("7d4f2c1e-9a3b-4e8d-b5f6-0c1a2b3c4d5e"),
            Level = 3,
            Quantity = 12,
            OptionalQuantity = 4,
            Population = 548_703,
            Ratio = 0.75,
            Weight = 23.5f,
            Price = 899.99m,
            Budget = 2500m,
            TitledText = "Coastal tour",
            FieldDescribedText = "Aisle",
            OverriddenDescription = "lisbon.trips",
            ReadOnlyText = "VT-2026-0042",
            ReadOnlyAmount = 1299.5m,
            PlaceholderText = "Window seat",
            ExplicitEmail = "concierge@voyager.example",
            TextQuantity = 2,
            Constrained = 125.25m,
            Domain = "voyager.example",
            Mass = 18.4m,
            GroupedPrice = 349m,
            StyledCode = "LIS-07",
        };
}
